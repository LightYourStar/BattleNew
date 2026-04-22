using Game.Battle.Runtime.Bootstrap;
using Game.Battle.Runtime.Core;

namespace Game.Battle.Hotfix.Bootstrap
{
    /// <summary>
    /// 热更运行器：继承 <see cref="BattleRunnerBehaviour"/>，覆盖两个钩子：
    /// <list type="bullet">
    ///   <item><see cref="SetupContext"/>：注册 HeroDef / WeaponDef / TraitFactory / TraitPool。</item>
    ///   <item><see cref="BuildLoadout"/>：构建本局 Loadout（英雄/武器/词条池/RNG 种子）。</item>
    /// </list>
    /// <para>
    /// 使用方式：场景中把 <c>HotfixBattleRunnerBehaviour</c> 挂在 GameObject 上，
    /// 替代原来的 <c>BattleRunnerBehaviour</c>；Inspector 配置完全兼容。
    /// </para>
    /// </summary>
    public sealed class HotfixBattleRunnerBehaviour : BattleRunnerBehaviour
    {
        // ─── Inspector：战前配置选择 ──────────────────────────────────────────

        [UnityEngine.Header("Battle Loadout")]
        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("英雄定义 Id（对应 HeroDefRegistry），留空使用内置默认英雄")]
        private string _heroDefId = "hero_warrior";

        [UnityEngine.SerializeField]
        [UnityEngine.Tooltip("武器定义 Id（对应 WeaponDefRegistry），留空使用英雄默认武器")]
        private string _weaponDefId = "";

        // ─── 钩子实现 ─────────────────────────────────────────────────────────

        /// <inheritdoc />
        protected override void SetupContext(BattleContext context)
        {
            HotfixAllRegistrar.RegisterAll(context);
        }

        /// <inheritdoc />
        protected override BattleLoadout BuildLoadout()
        {
            BattleLoadout loadout = new BattleLoadout
            {
                HeroDefId = string.IsNullOrEmpty(_heroDefId) ? null : _heroDefId,
                WeaponDefId = string.IsNullOrEmpty(_weaponDefId) ? null : _weaponDefId,
                // RngSeed = 0 → BattleWorld.SeedFromLoadout 自动填入时间戳种子
            };

            // 词条池：由 SeedFromLoadout 自动从 HeroDef.TraitPoolIds + WeaponDef.TraitPoolIds 合并
            // 若需要手动指定（例如关卡限定池），在此赋值 loadout.TraitPoolIds
            return loadout;
        }
    }
}
