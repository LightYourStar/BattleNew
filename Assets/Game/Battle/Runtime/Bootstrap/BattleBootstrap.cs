using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Hero;
using UnityEngine;

namespace Game.Battle.Runtime.Bootstrap
{
    /// <summary>
    /// 战斗启动器：只做生命周期编排（创建/启动/驱动/退出），不写具体战斗规则。
    /// <para>
    /// 典型挂载方式：
    /// - 由场景中的 <see cref="BattleRunnerBehaviour"/> 持有并每帧调用 <see cref="Update"/>。
    /// </para>
    /// </summary>
    public sealed class BattleBootstrap
    {
        /// <summary>当前战斗世界；未进入战斗时为 null。</summary>
        public BattleWorld? World { get; private set; }

        /// <summary>
        /// 进入战斗：创建（或注入）BattleWorld，播种最小实体，并启动固定帧循环。
        /// </summary>
        public void EnterBattle(BattleWorld? world = null)
        {
            World = world ?? new BattleWorld();
            SeedMinimalLoop(World.Context);
            World.Start();
        }

        /// <summary>外部驱动更新：用渲染 deltaTime 推进逻辑帧。</summary>
        public void Update(float deltaTime)
        {
            World?.Update(deltaTime);
        }

        /// <summary>退出战斗：停止世界并释放引用。</summary>
        public void ExitBattle()
        {
            if (World == null)
            {
                return;
            }

            World.Stop();
            World = null;
        }

        /// <summary>
        /// 播种最小闭环数据：当前仅创建单英雄，敌人由 <c>WaveSystem</c> 刷新。
        /// </summary>
        private static void SeedMinimalLoop(BattleContext context)
        {
            context.Registry.Heroes.Add(new HeroEntity("hero_1", Vector3.zero));
        }
    }
}
