namespace Game.Config.Adapters
{
    /// <summary>
    /// 旧角色数据到新配置的适配占位：集中处理字段改名、拆表、枚举映射等，避免业务散落魔法数。
    /// </summary>
    public sealed class OldRoleDataAdapter
    {
        /// <summary>将旧角色 Id 转为 Attr 表键（示例实现直接返回原值）。</summary>
        public int ToAttrId(int legacyRoleId)
        {
            return legacyRoleId;
        }
    }
}
