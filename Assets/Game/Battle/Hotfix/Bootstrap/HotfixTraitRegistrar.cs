using Game.Battle.Hotfix.Traits;
using Game.Battle.Runtime.Core;

namespace Game.Battle.Hotfix.Bootstrap
{
    /// <summary>
    /// 热更词条注册器：在战斗启动前把所有热更词条实现注册到 <see cref="BattleContext.TraitFactory"/>。
    /// <para>
    /// 调用时机：在 <c>BattleBootstrap.EnterBattle</c> 的 <c>setupContext</c> 回调中调用：
    /// <code>
    /// _bootstrap.EnterBattle(setupContext: HotfixTraitRegistrar.RegisterAll);
    /// </code>
    /// </para>
    /// <para>
    /// 扩展方式：每新增一个热更词条，只需在 <see cref="RegisterAll"/> 中添加一行
    /// <c>ctx.TraitFactory.Register(...)</c>，不需要改动 Runtime 层任何代码。
    /// </para>
    /// </summary>
    public static class HotfixTraitRegistrar
    {
        /// <summary>
        /// 注册全部热更词条到 TraitFactory。
        /// </summary>
        public static void RegisterAll(BattleContext ctx)
        {
            // "trait_damage_boost" → DamageBoostTrait(ownerId, rangeBonus=2, damageRatio=0.1)
            ctx.TraitFactory.Register(
                "trait_damage_boost",
                ownerId => new DamageBoostTrait(ownerId));
        }
    }
}
