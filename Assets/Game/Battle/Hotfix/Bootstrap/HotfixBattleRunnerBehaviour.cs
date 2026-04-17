using Game.Battle.Runtime.Bootstrap;
using Game.Battle.Runtime.Core;

namespace Game.Battle.Hotfix.Bootstrap
{
    /// <summary>
    /// 热更运行器：继承 <see cref="BattleRunnerBehaviour"/>，
    /// 覆盖 <see cref="SetupContext"/> 把所有热更词条/Buff 工厂注册到 Context。
    /// <para>
    /// 使用方式：场景中把 <c>HotfixBattleRunnerBehaviour</c> 挂在 GameObject 上，
    /// 替代原来的 <c>BattleRunnerBehaviour</c>，其余 Inspector 配置完全兼容。
    /// </para>
    /// </summary>
    public sealed class HotfixBattleRunnerBehaviour : BattleRunnerBehaviour
    {
        /// <inheritdoc />
        protected override void SetupContext(BattleContext context)
        {
            HotfixTraitRegistrar.RegisterAll(context);
        }
    }
}
