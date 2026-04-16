using System.Collections.Generic;
using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.AI;

namespace Game.Battle.Runtime.Entities.Wave
{
    /// <summary>
    /// 波次系统：驱动多波次刷怪流程。
    /// <para>
    /// 职责边界：
    /// <list type="bullet">
    ///   <item>判断"何时刷新"（清场后等待延迟到期）。</item>
    ///   <item>调用 SpawnSystem 决定"刷什么、刷多少、刷哪里"。</item>
    ///   <item>维护 <see cref="PendingWaveCount"/> 供胜负规则查询。</item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class WaveSystem
    {
        private readonly List<WaveConfig> _waves;
        private int _currentWaveIndex;
        private bool _currentWaveActive;
        private float _delayTimer;

        /// <summary>还未完成的波次数量（VictoryRule 用此判断全部清空）。</summary>
        public int PendingWaveCount => _waves.Count - _currentWaveIndex;

        /// <summary>
        /// 默认构造：单波次（向后兼容最小闭环验证）。
        /// </summary>
        public WaveSystem() : this(new List<WaveConfig> { new WaveConfig(0) })
        {
        }

        /// <summary>
        /// 配置化构造：传入完整波次列表。
        /// </summary>
        public WaveSystem(List<WaveConfig> waves)
        {
            _waves = waves;
        }

        /// <summary>
        /// 每逻辑帧更新：
        /// 1. 当前波次活跃时，检测是否已清场。
        /// 2. 清场后递减延迟计时器；延迟到期则触发下一波刷新。
        /// </summary>
        public void Tick(BattleContext context, float deltaTime)
        {
            if (_currentWaveIndex >= _waves.Count)
            {
                return;
            }

            if (_currentWaveActive)
            {
                if (IsWaveCleared(context))
                {
                    _currentWaveActive = false;
                    _currentWaveIndex++;

                    if (_currentWaveIndex < _waves.Count)
                    {
                        _delayTimer = _waves[_currentWaveIndex].DelaySeconds;
                    }
                }
                return;
            }

            // 等待延迟后刷新下一波
            if (_currentWaveIndex >= _waves.Count)
            {
                return;
            }

            _delayTimer -= deltaTime;
            if (_delayTimer > 0f)
            {
                return;
            }

            SpawnWave(context, _waves[_currentWaveIndex]);
            _currentWaveActive = true;
        }

        private void SpawnWave(BattleContext context, WaveConfig config)
        {
            List<AIEntity> enemies = context.SpawnSystem.SpawnWave(context, config.WaveIndex);
            for (int i = 0; i < enemies.Count; i++)
            {
                context.Registry.Enemies.Add(enemies[i]);
            }
        }

        private static bool IsWaveCleared(BattleContext context)
        {
            for (int i = 0; i < context.Registry.Enemies.Count; i++)
            {
                if (context.Registry.Enemies[i].IsAlive)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
