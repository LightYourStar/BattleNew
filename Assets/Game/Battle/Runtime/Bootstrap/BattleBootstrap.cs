using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Hero;
using Game.Battle.Runtime.Services.Replay;
using UnityEngine;

namespace Game.Battle.Runtime.Bootstrap
{
    /// <summary>
    /// 战斗启动器：只做生命周期编排（创建/启动/驱动/退出），不写具体战斗规则。
    /// <para>
    /// 同时支持两种模式：
    /// <list type="bullet">
    ///   <item>正常战斗：<see cref="EnterBattle"/> → <see cref="Update"/> → <see cref="ExitBattle"/></item>
    ///   <item>回放模式：<see cref="EnterReplay"/> → <see cref="Update"/> → <see cref="ExitBattle"/></item>
    /// </list>
    /// 外部驱动方（BattleRunnerBehaviour）无需区分模式，统一调用 <see cref="Update"/>。
    /// </para>
    /// </summary>
    public sealed class BattleBootstrap
    {
        /// <summary>当前战斗世界；未进入战斗时为 null。</summary>
        public BattleWorld? World { get; private set; }

        /// <summary>当前回放会话；非回放模式时为 null。</summary>
        public BattleReplaySession? ReplaySession { get; private set; }

        /// <summary>是否正在回放。</summary>
        public bool IsReplaying => ReplaySession != null;

        // ─── 正常战斗 ─────────────────────────────────────────────────────────

        /// <summary>
        /// 进入战斗：创建（或注入）BattleWorld，播种最小实体，并启动固定帧循环。
        /// </summary>
        public void EnterBattle(BattleWorld? world = null)
        {
            World = world ?? new BattleWorld();
            SeedMinimalLoop(World.Context);
            World.Start();
        }

        // ─── 回放模式 ─────────────────────────────────────────────────────────

        /// <summary>
        /// 进入回放：从 <see cref="ReplayRecord"/> 重建一局战斗并开始播放。
        /// <para>
        /// 关键设计：回放内部使用 <see cref="ReplayNetAdapter"/> 把录像命令注入 OrderBus，
        /// 走与正常战斗完全相同的命令消费链，无需改动 BattleWorld。
        /// </para>
        /// </summary>
        public void EnterReplay(ReplayRecord record, float playbackSpeed = 1f)
        {
            if (!record.IsSealed)
            {
                UnityEngine.Debug.LogWarning("[BattleBootstrap] ReplayRecord 尚未封存（Seal），无法回放。");
                return;
            }

            ReplaySession = new BattleReplaySession(record);
            ReplaySession.PlaybackSpeed = playbackSpeed;
            ReplaySession.Start();

            // 把回放世界也赋给 World，供外部（如可视化器）统一访问
            World = ReplaySession.World;
        }

        // ─── 公共驱动 ─────────────────────────────────────────────────────────

        /// <summary>
        /// 外部驱动更新：正常战斗与回放模式统一入口。
        /// </summary>
        public void Update(float deltaTime)
        {
            if (ReplaySession != null)
            {
                ReplaySession.Update(deltaTime);
            }
            else
            {
                World?.Update(deltaTime);
            }
        }

        /// <summary>退出战斗（或回放）：停止世界并释放引用。</summary>
        public void ExitBattle()
        {
            if (ReplaySession != null)
            {
                ReplaySession.Finish();
                ReplaySession = null;
            }
            else if (World != null)
            {
                World.Stop();
            }

            World = null;
        }

        // ─── 私有 ─────────────────────────────────────────────────────────────

        /// <summary>
        /// 播种最小闭环数据：当前仅创建单英雄，敌人由 <c>WaveSystem</c> 刷新。
        /// </summary>
        private static void SeedMinimalLoop(BattleContext context)
        {
            context.Registry.Heroes.Add(new HeroEntity("hero_1", Vector3.zero));
        }
    }
}
