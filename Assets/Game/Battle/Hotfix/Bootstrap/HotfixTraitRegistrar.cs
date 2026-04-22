using Game.Battle.Hotfix.Traits;
using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Trait;

namespace Game.Battle.Hotfix.Bootstrap
{
    /// <summary>
    /// 热更词条注册器：在战斗启动前向 Context 注册所有热更词条实现与词条池。
    /// <para>
    /// 调用时机：通过 <c>BattleBootstrap.EnterBattle(setupContext)</c> 注入，
    /// 或在 <see cref="HotfixAllRegistrar.RegisterAll"/> 中统一调用。
    /// </para>
    /// <para>
    /// 扩展方式：每新增一个词条，在 <see cref="RegisterTraits"/> 中加一行 Register；
    /// 每新增一个池，在 <see cref="RegisterPools"/> 中加一行 Register。
    /// </para>
    /// </summary>
    public static class HotfixTraitRegistrar
    {
        /// <summary>注册所有热更词条工厂函数到 TraitFactory。</summary>
        public static void RegisterTraits(BattleContext ctx)
        {
            // "trait_damage_boost" → DamageBoostTrait(ownerId, rangeBonus=2, damageRatio=0.1)
            ctx.TraitFactory.Register(
                "trait_damage_boost",
                ownerId => new DamageBoostTrait(ownerId));
        }

        /// <summary>注册所有热更词条池到 TraitPoolRegistry。</summary>
        public static void RegisterPools(BattleContext ctx)
        {
            // 战士英雄的词条池：近战加伤系
            ctx.TraitPoolRegistry.Register(
                TraitPoolDef.Uniform("pool_warrior",
                    "trait_damage_boost"));

            // 长弓武器的词条池：远程增幅系（示例，词条可与英雄池相同也可独立）
            ctx.TraitPoolRegistry.Register(
                TraitPoolDef.Uniform("pool_bow",
                    "trait_damage_boost"));
        }
    }
}
