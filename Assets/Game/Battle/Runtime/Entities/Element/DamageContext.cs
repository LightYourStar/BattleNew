namespace Game.Battle.Runtime.Entities.Element
{
    /// <summary>
    /// 伤害上下文：一次伤害事件的全部参数，供管线各环节读取/修改。
    /// </summary>
    public sealed class DamageContext
    {
        public string AttackerId { get; }
        public string TargetId { get; }
        public float BaseDamage { get; }

        /// <summary>最终伤害（经过 Buff/词条修正后的值）。</summary>
        public float FinalDamage { get; set; }

        /// <summary>是否被闪避/格挡等判定为无效。</summary>
        public bool IsCancelled { get; set; }

        public DamageContext(string attackerId, string targetId, float baseDamage)
        {
            AttackerId = attackerId;
            TargetId = targetId;
            BaseDamage = baseDamage;
            FinalDamage = baseDamage;
        }
    }
}
