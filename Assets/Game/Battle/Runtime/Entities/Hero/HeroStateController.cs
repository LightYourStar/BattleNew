using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Entities.Hero
{
    /// <summary>
    /// 英雄状态枚举。
    /// </summary>
    public enum HeroState
    {
        Idle = 0,
        Moving = 1,
        Attacking = 2,
    }

    /// <summary>
    /// 英雄状态控制器：把移动/攻击/待机的切换逻辑集中管理，并产生 TraceStateChange。
    /// </summary>
    public sealed class HeroStateController
    {
        public void UpdateState(BattleContext context, HeroEntity hero, bool hasTarget, bool isMoving)
        {
            HeroState next;
            if (isMoving)
            {
                next = HeroState.Moving;
            }
            else if (hasTarget)
            {
                next = HeroState.Attacking;
            }
            else
            {
                next = HeroState.Idle;
            }

            if (hero.CurrentState == next)
            {
                return;
            }

            context.DebugTraceService.TraceStateChange(hero.Id, hero.CurrentState.ToString(), next.ToString());
            hero.CurrentState = next;
        }
    }
}
