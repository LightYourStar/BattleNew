using System;

namespace Game.Battle.Runtime.Core
{
    /// <summary>
    /// Fixed-step simulation loop; render and logic are decoupled.
    /// </summary>
    public sealed class FrameLoop
    {
        private float _accumulator;

        public float FixedStep { get; }

        public bool IsRunning { get; private set; }

        public bool IsPaused { get; private set; }

        public Action<float>? OnFixedTick;

        public FrameLoop(float fixedStep)
        {
            FixedStep = fixedStep;
        }

        public void Start()
        {
            _accumulator = 0f;
            IsRunning = true;
            IsPaused = false;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }

        public void Stop()
        {
            IsRunning = false;
            IsPaused = false;
            _accumulator = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning || IsPaused)
            {
                return;
            }

            _accumulator += deltaTime;
            while (_accumulator >= FixedStep)
            {
                _accumulator -= FixedStep;
                OnFixedTick?.Invoke(FixedStep);
            }
        }
    }
}
