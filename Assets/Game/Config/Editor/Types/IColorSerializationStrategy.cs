#if UNITY_EDITOR
using UnityEngine;

namespace Game.Config.Editor.Types
{
    /// <summary>
    /// 预留：Color 与 int 互转（compress_color_into_int 开关对接点）。
    /// </summary>
    public interface IColorSerializationStrategy
    {
        int ColorToInt(Color c);

        Color IntToColor(int packed);
    }
}
#endif
