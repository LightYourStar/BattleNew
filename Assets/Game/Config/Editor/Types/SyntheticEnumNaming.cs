#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;

namespace Game.Config.Editor.Types
{
    /// <summary>合成枚举成员名：校验与生成共用，避免命名漂移。</summary>
    public static class SyntheticEnumNaming
    {
        /// <summary>用于碰撞检测的稳定键（不做全局去重后缀）。</summary>
        public static string BaseMemberKey(string raw)
        {
            var sb = new StringBuilder();
            foreach (var ch in raw.Trim())
            {
                if (char.IsLetterOrDigit(ch) || ch == '_')
                {
                    sb.Append(ch);
                }
                else
                {
                    sb.Append('_');
                }
            }

            var name = sb.ToString();
            if (name.Length == 0 || char.IsDigit(name[0]))
            {
                name = "E_" + name;
            }

            return name;
        }

        /// <summary>生成最终成员名（在 <paramref name="taken"/> 内唯一）。</summary>
        public static string AllocateMemberName(string raw, HashSet<string> taken)
        {
            var baseName = BaseMemberKey(raw);
            var name = baseName;
            var i = 2;
            while (taken.Contains(name))
            {
                name = baseName + "_" + i;
                i++;
            }

            taken.Add(name);
            return name;
        }
    }
}
#endif
