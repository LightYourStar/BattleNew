using Game.Battle.Runtime.Commands;
using Game.Battle.Runtime.Commands.Commands;
using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Services.DebugTrace;
using UnityEngine;

namespace Game.Battle.Runtime.Bootstrap
{
    /// <summary>
    /// 场景级运行器：把 Unity 的 Update 时间轴接入 BattleBootstrap，并提供最小输入转命令示例。
    /// <para>
    /// 重要：该类属于“接入层”，不是战斗核心逻辑；核心仍应保持可脱离 Unity 运行与单测。
    /// </para>
    /// </summary>
    public sealed class BattleRunnerBehaviour : MonoBehaviour
    {
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

        private readonly BattleBootstrap _bootstrap = new();

        /// <summary>
        /// 防止同一逻辑帧重复注入输入命令（否则在高帧率下可能一帧堆多条 Move）。
        /// </summary>
        private int _lastInputCommandFrame = -1;

        /// <summary>暴露当前 BattleWorld，供表现层（例如可视化脚本）只读访问。</summary>
        public BattleWorld? World => _bootstrap.World;

        private void Awake()
        {
            if (_autoStartOnAwake)
            {
                StartBattle();
            }
        }

        /// <summary>
        /// 每渲染帧：
        /// 1) 尝试把输入写入命令总线
        /// 2) 推进战斗逻辑（内部会按固定步长切分）
        /// </summary>
        private void Update()
        {
            if (_bootstrap.World == null)
            {
                return;
            }

            PushInputCommand(_bootstrap.World.Context);
            _bootstrap.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _bootstrap.ExitBattle();
        }

        /// <summary>
        /// 手动启动战斗：创建带 Unity 日志追踪的 BattleWorld，并进入战斗。
        /// </summary>
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
            _bootstrap.EnterBattle(world);
            _lastInputCommandFrame = -1;
        }

        /// <summary>手动停止战斗。</summary>
        [ContextMenu("Stop Battle")]
        public void StopBattle()
        {
            _bootstrap.ExitBattle();
            _lastInputCommandFrame = -1;
        }

        /// <summary>
        /// 将轴输入转换为 <see cref="MoveCommand"/> 推入 <see cref="Game.Battle.Runtime.Commands.OrderBus"/>。
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
