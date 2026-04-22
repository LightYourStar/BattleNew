using System;
using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Services.Replay;

namespace Game.Battle.Runtime.Bootstrap
{
    /// <summary>
    /// 战斗启动器：只做生命周期编排（创建/启动/驱动/退出），不写具体战斗规则。
    /// <para>
    /// 支持两种模式：
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
        /// 进入战斗：创建（或注入）BattleWorld，调用 setupContext（注册 HeroDef/WeaponDef/Trait 等），
        /// 再按 Loadout 播种实体，最后启动固定帧循环。
        /// </summary>
        /// <param name="world">可选：外部注入的 BattleWorld（默认创建新实例）。</param>
        /// <param name="loadout">
        /// 战斗配置单（英雄、武器、词条池、RNG 种子）。
        /// 为 null 时退化为内置默认英雄 + 默认武器（最小闭环）。
        /// </param>
        /// <param name="setupContext">
        /// 可选：Context 构建完成后、播种之前的钩子回调。
        /// 在此注册 HeroDef / WeaponDef / TraitFactory / TraitPool 等，保持 Runtime 层无感知。
        /// </param>
        public void EnterBattle(
            BattleWorld? world = null,
            BattleLoadout? loadout = null,
            Action<BattleContext>? setupContext = null)
        {
            World = world ?? new BattleWorld();

            // 先让 Hotfix/外部层填充注册表
            setupContext?.Invoke(World.Context);

            // 再播种实体（依赖注册表内容）
            World.SeedFromLoadout(loadout ?? new BattleLoadout());

            // 将 Loadout 存入 ReplayRecord（便于后续导出录像时携带）
            World.Context.ReplayService.SetLoadout(World.Context.Loadout);

            World.Start();
        }

        // ─── 回放模式 ─────────────────────────────────────────────────────────

        /// <summary>
        /// 进入回放：从 <see cref="ReplayRecord"/> 重建一局战斗并开始播放。
        /// <para>
        /// 关键：<see cref="ReplayRecord.Loadout"/> 中的英雄/武器/种子与录制时完全一致，
        /// 确保回放的初始状态不偏离。
        /// </para>
        /// </summary>
        /// <param name="record">已封存的录像（必须调用过 <c>Seal</c>）。</param>
        /// <param name="playbackSpeed">回放速度倍率（默认 1.0 = 原速）。</param>
        /// <param name="setupContext">
        /// 可选：与录制时相同的注册回调（注册 HeroDef/WeaponDef/TraitFactory 等）。
        /// 若不传，词条命令在回放时会静默失败。
        /// </param>
        public void EnterReplay(
            ReplayRecord record,
            float playbackSpeed = 1f,
            Action<BattleContext>? setupContext = null)
        {
            if (!record.IsSealed)
            {
                UnityEngine.Debug.LogWarning("[BattleBootstrap] ReplayRecord 尚未封存（Seal），无法回放。");
                return;
            }

            ReplaySession = new BattleReplaySession(record, setupContext: setupContext);
            ReplaySession.PlaybackSpeed = playbackSpeed;
            ReplaySession.Start();

            // 把回放世界也赋给 World，供外部（如可视化器）统一访问
            World = ReplaySession.World;
        }

        // ─── 公共驱动 ─────────────────────────────────────────────────────────

        /// <summary>外部驱动更新：正常战斗与回放模式统一入口。</summary>
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
    }
}
