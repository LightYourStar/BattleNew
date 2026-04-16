using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Entities.Element
{
    /// <summary>
    /// 受击反应服务：击退、受击硬直、无敌判定等，独立于纯伤害数值计算。
    /// 当前为骨架占位。
    /// </summary>
    public sealed class HitReactionService
    {
        public void ProcessReaction(BattleContext context, DamageContext damageCtx)
        {
            // 后续在这里处理击退距离、硬直状态切换、无敌帧等
        }
    }
}
