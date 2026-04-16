using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.AI;
using Game.Battle.Runtime.Entities.Hero;

namespace Game.Battle.Runtime.Entities.Element
{
    /// <summary>
    /// 伤害服务：统一伤害入口，英雄和怪物共用一套伤害管线。
    /// <para>
    /// 管线顺序：
    /// 1. 构造 DamageContext（含 BaseDamage = FinalDamage 初始值）。
    /// 2. BuffSystem.ModifyDamage → 活跃 Buff 修正伤害/取消。
    /// 3. TraitSystem.ModifyDamage → 词条修正伤害/取消。
    /// 4. 若未被取消，找到目标实体，扣血，触发死亡事件。
    /// </para>
    /// </summary>
    public sealed class DamageService
    {
        /// <summary>
        /// 对目标施加伤害。
        /// </summary>
        public void ApplyDamage(BattleContext context, string attackerId, string targetId, float baseDamage)
        {
            DamageContext damageCtx = new(attackerId, targetId, baseDamage);

            // Buff 修正（攻击方增伤、受击方减伤/免疫等）
            context.BuffSystem.ModifyDamage(damageCtx);

            // 词条修正（攻击方伤害加成、受击方护盾等）
            context.TraitSystem.ModifyDamage(damageCtx);

            if (damageCtx.IsCancelled)
            {
                return;
            }

            // 尝试扣敌人血
            for (int i = 0; i < context.Registry.Enemies.Count; i++)
            {
                AIEntity enemy = context.Registry.Enemies[i];
                if (enemy.Id == targetId && enemy.IsAlive)
                {
                    enemy.ReceiveDamage(damageCtx.FinalDamage);
                    context.DebugTraceService.TraceDamage(attackerId, targetId, damageCtx.FinalDamage);
                    if (!enemy.IsAlive)
                    {
                        context.DeathService.OnEntityDeath(context, enemy.Id);
                    }
                    return;
                }
            }

            // 尝试扣英雄血
            for (int i = 0; i < context.Registry.Heroes.Count; i++)
            {
                HeroEntity hero = context.Registry.Heroes[i];
                if (hero.Id == targetId)
                {
                    hero.CurrentHp -= damageCtx.FinalDamage;
                    if (hero.CurrentHp < 0f)
                    {
                        hero.CurrentHp = 0f;
                    }
                    context.DebugTraceService.TraceDamage(attackerId, targetId, damageCtx.FinalDamage);
                    return;
                }
            }
        }
    }
}
