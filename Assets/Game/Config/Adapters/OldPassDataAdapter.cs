namespace Game.Config.Adapters
{
    /// <summary>
    /// 旧通行证数据到新配置的适配占位：把旧系统 ID 映射为新表主键或行索引（此处为恒等示例）。
    /// </summary>
    public sealed class OldPassDataAdapter
    {
        /// <summary>将旧通行证 Id 转为 Attr 表可用的键（示例实现直接返回原值）。</summary>
        public int ToAttrId(int legacyPassId)
        {
            return legacyPassId;
        }
    }
}
