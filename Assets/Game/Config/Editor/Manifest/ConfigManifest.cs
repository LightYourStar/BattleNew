#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Config.Editor.Manifest
{
    [Serializable]
    public sealed class ConfigTableEntry
    {
        public string TableName;
        public bool Enabled = true;
        public bool UseCustomLayout;
        public int HeaderRowIndex = 0;
        public int TypeRowIndex = 1;
        public int DataStartRowIndex = 2;
        public bool AutoDetectTypeRowWhenEmpty = true;
    }

    /// <summary>
    /// 配置生成清单（manifest）：CI 与本地生成统一以该文件为真相来源。
    /// </summary>
    public sealed class ConfigManifest : ScriptableObject
    {
        public int GlobalHeaderRowIndex = 0;
        public int GlobalTypeRowIndex = 1;
        public int GlobalDataStartRowIndex = 2;
        public bool GlobalAutoDetectTypeRowWhenEmpty = true;
        public List<ConfigTableEntry> Tables = new List<ConfigTableEntry>();
    }
}
#endif
