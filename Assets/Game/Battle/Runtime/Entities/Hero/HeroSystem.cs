using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Entities.Hero
{
    /// <summary>
    /// 英雄系统：英雄域的每帧后处理入口（当前为占位）。
    /// <para>
    /// 各能力已拆分到独立类：
    /// <list type="bullet">
    ///   <item>位置推进 + 朝向 → <see cref="HeroMovementController"/></item>
    ///   <item>锁敌        → <see cref="HeroTargetingService"/></item>
    ///   <item>状态切换    → <see cref="HeroStateController"/></item>
    ///   <item>武器发射    → WeaponFireService</item>
    /// </list>
    /// HeroSystem 预留给：技能 Tick、无敌帧倒计时、护盾衰减等需要"每帧对英雄做全量扫描"的逻辑。
    /// </para>
    /// </summary>
    public sealed class HeroSystem
    {
        /// <summary>每逻辑帧后处理（当前为占位）。</summary>
        public void Tick(BattleContext context, float deltaTime)
        {
        }
    }
}
