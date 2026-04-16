using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.AI;
using Game.Battle.Runtime.Entities.Hero;

namespace Game.Battle.Runtime.Entities.Element
{
    /// <summary>
    /// 伤害服务：统一伤害入口，英雄和怪物共用一套伤害管线。
    /// <para>
    /// 管线顺序：
    /// 1. 构造 DamageContext（BaseDamage = FinalDamage 初始值）。
    /// 2. HitReactionService.IsInvincible → 目标处于无敌帧则取消（IsCancelled = true）。
    /// 3. BuffSystem.ModifyDamage → 活跃 Buff 修正伤害/取消。
    /// 4. TraitSystem.ModifyDamage → 词条修正伤害/取消。
    /// 5. 若未被取消，找到目标实体，扣血，触发死亡事件。
    /// </para>
    /// <para>
    /// 受击反应（击退/硬直/无敌帧）由调用方在确认命中后通过
    /// <see cref="HitReactionService.ProcessHit"/> 单独处理，不在本类内处理。
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

            // 无敌帧检查（英雄专属，受击反应的一部分，优先于 Buff/Trait 修正）
            if (context.HitReactionService.IsInvincible(context, targetId))
            {
                damageCtx.IsCancelled = true;
                return;
            }

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
