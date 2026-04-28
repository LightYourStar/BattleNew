#if UNITY_EDITOR
namespace Game.Config.Editor.Types
{
    /// <summary>
    /// 预留：string / string[] 哈希或索引化存储（use_hash_string 开关对接点）。
    /// </summary>
    public interface IStringHashProvider
    {
        int Hash(string value);
    }
}
#endif
