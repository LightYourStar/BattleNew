using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Element;

namespace Game.Battle.Runtime.Entities.Buff
{
    /// <summary>
    /// Buff 系统：持有 BuffRegistry，在每逻辑帧推进所有活跃 Buff，
    /// 同时为 DamageService 提供伤害修正钩子。
    /// </summary>
    public sealed class BuffSystem
    {
        public BuffRegistry Registry { get; } = new();

        /// <summary>每逻辑帧更新所有活跃 Buff（到期自动移除）。</summary>
        public void Tick(BattleContext context, float deltaTime)
        {
            Registry.Tick(context, deltaTime);
        }

        /// <summary>添加 Buff。</summary>
        public void AddBuff(BattleContext context, IBuff buff)
        {
            Registry.Add(context, buff);
        }

        /// <summary>手动移除 Buff。</summary>
        public void RemoveBuff(BattleContext context, IBuff buff)
        {
            Registry.Remove(context, buff);
        }

        /// <summary>
        /// 遍历所有与本次伤害相关的 Buff（攻击方或受击方），调用其伤害修正钩子。
        /// DamageService 在计算最终伤害前调用此方法。
        /// </summary>
        public void ModifyDamage(DamageContext damageCtx)
        {
            var buffs = Registry.ActiveBuffs;
            for (int i = 0; i < buffs.Count; i++)
            {
                IBuff buff = buffs[i];
                if (buff.OwnerId == damageCtx.AttackerId || buff.OwnerId == damageCtx.TargetId)
                {
                    buff.ModifyDamage(damageCtx);
                }
            }
        }
    }
}
