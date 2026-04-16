using Game.Battle.Runtime.Commands;
using Game.Battle.Runtime.Commands.Commands;
using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Services.DebugTrace;
using UnityEngine;

namespace Game.Battle.Runtime.Bootstrap
{
    /// <summary>
    /// Scene-level runner to validate the minimal battle loop.
    /// </summary>
    public sealed class BattleRunnerBehaviour : MonoBehaviour
    {
        [SerializeField]
        private bool _autoStartOnAwake = true;

        [SerializeField]
        private bool _logFrameAdvance;

        [SerializeField]
        private bool _logCommandConsumed;

        [SerializeField]
        private string _heroId = "hero_1";

        private readonly BattleBootstrap _bootstrap = new();
        private int _lastInputCommandFrame = -1;

        private void Awake()
        {
            if (_autoStartOnAwake)
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

            PushInputCommand(_bootstrap.World.Context);
            _bootstrap.Update(Time.deltaTime);
        }

        private void OnDestroy()
        {
            _bootstrap.ExitBattle();
        }

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

        [ContextMenu("Stop Battle")]
        public void StopBattle()
        {
            _bootstrap.ExitBattle();
            _lastInputCommandFrame = -1;
        }

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
