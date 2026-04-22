using Game.Battle.Runtime.Core;

namespace Game.Battle.Hotfix.Bootstrap
{
    /// <summary>
    /// 热更总注册入口：一次性调用所有 Hotfix 注册器。
    /// <para>
    /// 使用方式：
    /// <code>
    /// _bootstrap.EnterBattle(
    ///     loadout: myLoadout,
    ///     setupContext: HotfixAllRegistrar.RegisterAll);
    /// </code>
    /// </para>
    /// </summary>
    public static class HotfixAllRegistrar
    {
        public static void RegisterAll(BattleContext ctx)
        {
            HotfixDefRegistrar.RegisterHeroes(ctx);
            HotfixDefRegistrar.RegisterWeapons(ctx);
            HotfixTraitRegistrar.RegisterTraits(ctx);
            HotfixTraitRegistrar.RegisterPools(ctx);
        }
    }
}
