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
    /// 使用方式：
    /// <list type="bullet">
    ///   <item>将 <c>HotfixBattleRunnerBehaviour</c>（Hotfix 子类）挂到场景 GameObject，
    ///         它会覆盖 <see cref="SetupContext"/> 和 <see cref="BuildLoadout"/> 注入具体定义。</item>
    ///   <item>本类本身是 Runtime 层，不引用任何 Hotfix 类型，保持边界清晰。</item>
    /// </list>
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
        [Tooltip("勾选后将在 Awake 时进入回放模式（需先通过 StopBattle 存储录像）")]
        [SerializeField]
        private bool _startAsReplay;

        [Tooltip("回放速度倍率（1.0 = 原速，2.0 = 双倍速，0.5 = 半速）")]
        [SerializeField]
        [Range(0.1f, 8f)]
        private float _replaySpeed = 1f;

        // ─── 私有状态 ─────────────────────────────────────────────────────────

        private readonly BattleBootstrap _bootstrap = new();

        /// <summary>上次战斗录像（战斗结束后自动保存，用于下次回放）。</summary>
        private ReplayRecord? _lastRecord;

        /// <summary>防止同一逻辑帧重复注入输入命令。</summary>
        private int _lastInputCommandFrame = -1;

        // ─── 公开属性 ─────────────────────────────────────────────────────────

        public BattleWorld? World => _bootstrap.World;
        public bool IsReplaying => _bootstrap.IsReplaying;

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
            BattleLoadout loadout = BuildLoadout();

            _bootstrap.EnterBattle(world, loadout, SetupContext);
            _lastInputCommandFrame = -1;
        }

        /// <summary>停止战斗并保存录像。</summary>
        [ContextMenu("Stop Battle & Save Replay")]
        public void StopBattle()
        {
            if (_bootstrap.World != null && !_bootstrap.IsReplaying)
            {
                _lastRecord = _bootstrap.World.Context.ReplayService.ExportRecord();
                Debug.Log($"[BattleRunner] 录像已保存，共 {_lastRecord.CommandFrameCount} 个命令帧，" +
                          $"最大帧号 {_lastRecord.MaxFrame}，" +
                          $"英雄={_lastRecord.Loadout?.HeroDefId ?? "default"}，" +
                          $"种子={_lastRecord.Loadout?.RngSeed}。");
            }

            _bootstrap.ExitBattle();
            _lastInputCommandFrame = -1;
        }

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

        public void StartReplay(ReplayRecord record)
        {
            if (_bootstrap.World != null)
            {
                _bootstrap.ExitBattle();
            }

            Debug.Log($"[BattleRunner] 开始回放，英雄={record.Loadout?.HeroDefId ?? "default"}，" +
                      $"共 {record.CommandFrameCount} 个命令帧，速度 {_replaySpeed}x。");

            _bootstrap.EnterReplay(record, _replaySpeed, SetupContext);
        }

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

        // ─── 可被 Hotfix 子类覆盖的钩子 ──────────────────────────────────────

        /// <summary>
        /// 注册 HeroDef / WeaponDef / TraitFactory / TraitPool 等的钩子。
        /// 子类在此填充注册表，Runtime 层默认为空实现，不引用任何 Hotfix 类型。
        /// </summary>
        protected virtual void SetupContext(BattleContext context)
        {
        }

        /// <summary>
        /// 构建本局 Loadout（英雄选择 / 武器选择 / 词条池）的钩子。
        /// 子类覆盖以返回玩家在战前 UI 中的选择；Runtime 层默认返回空 Loadout（内置默认值）。
        /// </summary>
        protected virtual BattleLoadout BuildLoadout()
        {
            return new BattleLoadout();
        }

        // ─── 私有 ─────────────────────────────────────────────────────────────

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
