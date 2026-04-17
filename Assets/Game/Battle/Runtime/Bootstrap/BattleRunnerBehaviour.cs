using Game.Battle.Runtime.Commands;
using Game.Battle.Runtime.Commands.Commands;
using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Services.DebugTrace;
using Game.Battle.Runtime.Services.Replay;
using UnityEngine;

namespace Game.Battle.Runtime.Bootstrap
{
    /// <summary>
    /// 场景级运行器：把 Unity 的 Update 时间轴接入 BattleBootstrap，
    /// 同时支持"正常战斗"和"回放模式"，外部只需在 Inspector 中切换 <see cref="_startAsReplay"/>。
    /// <para>
    /// 重要：该类属于"接入层"，不是战斗核心逻辑；核心应保持可脱离 Unity 运行与单测。
    /// </para>
    /// </summary>
    public class BattleRunnerBehaviour : MonoBehaviour
    {
        // ─── Inspector 配置 ───────────────────────────────────────────────────

        [Header("Lifecycle")]
        [SerializeField]
        private bool _autoStartOnAwake = true;

        [Header("Debug Trace")]
        [SerializeField]
        private bool _logFrameAdvance;

        [SerializeField]
        private bool _logCommandConsumed;

        [Header("Input -> Command")]
        [SerializeField]
        private string _heroId = "hero_1";

        [Header("Replay Mode")]
        [Tooltip("勾选后将在 Awake 时进入回放模式（需先通过 SaveLastReplay 存储录像）")]
        [SerializeField]
        private bool _startAsReplay;

        [Tooltip("回放速度倍率（1.0 = 原速，2.0 = 双倍速，0.5 = 半速）")]
        [SerializeField]
        [Range(0.1f, 8f)]
        private float _replaySpeed = 1f;

        // ─── 私有状态 ─────────────────────────────────────────────────────────

        private readonly BattleBootstrap _bootstrap = new();

        /// <summary>上次战斗录像（用于下次回放；正常战斗结束后自动保存）。</summary>
        private ReplayRecord? _lastRecord;

        /// <summary>防止同一逻辑帧重复注入输入命令。</summary>
        private int _lastInputCommandFrame = -1;

        // ─── 公开属性 ─────────────────────────────────────────────────────────

        /// <summary>暴露当前 BattleWorld，供表现层（例如可视化脚本）只读访问。</summary>
        public BattleWorld? World => _bootstrap.World;

        /// <summary>当前是否在回放。</summary>
        public bool IsReplaying => _bootstrap.IsReplaying;

        /// <summary>回放进度 0–1；非回放时为 0。</summary>
        public float ReplayProgress
        {
            get
            {
                BattleReplaySession? session = _bootstrap.ReplaySession;
                if (session == null || session.TotalFrames <= 0)
                {
                    return 0f;
                }
                return (float)session.CurrentFrame / session.TotalFrames;
            }
        }

        // ─── Unity 生命周期 ───────────────────────────────────────────────────

        private void Awake()
        {
            if (!_autoStartOnAwake)
            {
                return;
            }

            if (_startAsReplay && _lastRecord != null)
            {
                StartReplay(_lastRecord);
            }
            else
            {
                StartBattle();
            }
        }

        private void Update()
        {
            if (_bootstrap.World == null)
            {
                return;
            }

            // 回放模式下不注入本地输入
            if (!_bootstrap.IsReplaying)
            {
                PushInputCommand(_bootstrap.World.Context);
            }

            _bootstrap.Update(Time.deltaTime);

            // 正常战斗结束后自动保存录像
            if (!_bootstrap.IsReplaying && _bootstrap.World == null)
            {
                // World 变成 null 意味着战斗已由内部 Stop 触发退出；录像已在 ExitBattle 前导出
            }

            // 回放播放完毕后自动退出
            if (_bootstrap.ReplaySession is { IsFinished: true })
            {
                _bootstrap.ExitBattle();
                Debug.Log("[BattleRunner] 回放播放完毕。");
            }
        }

        private void OnDestroy()
        {
            // 退出前导出当前录像（方便下次 Awake 用于回放）
            if (!_bootstrap.IsReplaying && _bootstrap.World != null)
            {
                _lastRecord = _bootstrap.World.Context.ReplayService.ExportRecord();
            }

            _bootstrap.ExitBattle();
        }

        // ─── 公开控制方法（可从 UI Button / ContextMenu 调用）────────────────

        /// <summary>启动正常战斗。</summary>
        [ContextMenu("Start Battle")]
        public void StartBattle()
        {
            if (_bootstrap.World != null)
            {
                return;
            }

            IDebugTraceService debugTraceService = new UnityDebugTraceService(
                logFrameAdvance: _logFrameAdvance,
                logCommandConsumed: _logCommandConsumed);

            BattleWorld world = new BattleWorld(debugTraceService: debugTraceService);
            _bootstrap.EnterBattle(world, setupContext: SetupContext);
            _lastInputCommandFrame = -1;
        }

        /// <summary>
        /// Context 构建完成后、战斗启动前的钩子：子类在此注册热更词条/Buff 工厂等内容。
        /// <para>
        /// 默认为空，Hotfix 子类覆盖即可，无需改动任何 Runtime 代码。
        /// 例：<c>context.TraitFactory.Register("trait_damage_boost", id => new DamageBoostTrait(id));</c>
        /// </para>
        /// </summary>
        protected virtual void SetupContext(BattleContext context)
        {
        }

        /// <summary>停止战斗并保存录像。</summary>
        [ContextMenu("Stop Battle & Save Replay")]
        public void StopBattle()
        {
            if (_bootstrap.World != null && !_bootstrap.IsReplaying)
            {
                _lastRecord = _bootstrap.World.Context.ReplayService.ExportRecord();
                Debug.Log($"[BattleRunner] 录像已保存，共 {_lastRecord.CommandFrameCount} 个命令帧，最大帧号 {_lastRecord.MaxFrame}。");
            }

            _bootstrap.ExitBattle();
            _lastInputCommandFrame = -1;
        }

        /// <summary>
        /// 用上一局保存的录像开始回放。
        /// </summary>
        [ContextMenu("Replay Last Battle")]
        public void ReplayLastBattle()
        {
            if (_lastRecord == null)
            {
                Debug.LogWarning("[BattleRunner] 没有可回放的录像，请先完成一局战斗。");
                return;
            }

            StartReplay(_lastRecord);
        }

        /// <summary>
        /// 从指定录像开始回放。
        /// </summary>
        public void StartReplay(ReplayRecord record)
        {
            if (_bootstrap.World != null)
            {
                _bootstrap.ExitBattle();
            }

            Debug.Log($"[BattleRunner] 开始回放，共 {record.CommandFrameCount} 个命令帧，最大帧号 {record.MaxFrame}，速度 {_replaySpeed}x。");
            _bootstrap.EnterReplay(record, _replaySpeed);
        }

        /// <summary>暂停/继续回放。</summary>
        [ContextMenu("Toggle Replay Pause")]
        public void ToggleReplayPause()
        {
            if (_bootstrap.ReplaySession == null)
            {
                return;
            }

            if (_bootstrap.World?.FrameLoop.IsPaused == false)
            {
                _bootstrap.ReplaySession.Pause();
                Debug.Log("[BattleRunner] 回放已暂停。");
            }
            else
            {
                _bootstrap.ReplaySession.Resume();
                Debug.Log("[BattleRunner] 回放已继续。");
            }
        }

        // ─── 私有 ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 将轴输入转换为 <see cref="MoveCommand"/> 推入 <see cref="OrderBus"/>。
        /// </summary>
        private void PushInputCommand(BattleContext context)
        {
            if (_lastInputCommandFrame == context.Time.Frame)
            {
                return;
            }

            Vector3 input = new Vector3(
                Input.GetAxisRaw("Horizontal"),
                0f,
                Input.GetAxisRaw("Vertical"));

            if (input.sqrMagnitude <= 0f)
            {
                return;
            }

            MoveCommand moveCommand = new(
                frame: context.Time.Frame,
                source: CommandSource.LocalInput,
                heroId: _heroId,
                direction: input);

            context.OrderBus.PushLocalCommand(moveCommand);
            _lastInputCommandFrame = context.Time.Frame;
        }
    }
}
