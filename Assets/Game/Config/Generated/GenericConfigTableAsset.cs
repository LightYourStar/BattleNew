using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Config.Generated
{
    /// <summary>
    /// 通用配置资产：用于在“类已生成/待编译”阶段也能稳定导出 .asset 原始数据。
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Config/GenericConfigTableAsset", fileName = "GenericConfigTableAsset")]
    public sealed class GenericConfigTableAsset : ScriptableObject
    {
        public string TableName;
        public List<string> Columns = new List<string>();
        public List<string> TypeTokens = new List<string>();
        public List<GenericConfigRow> Rows = new List<GenericConfigRow>();

        public void ResetFrom(
            string tableName,
            List<string> columns,
            List<string> typeTokens,
            List<GenericConfigRow> rows)
        {
            TableName = tableName ?? string.Empty;
            Columns = columns ?? new List<string>();
            TypeTokens = typeTokens ?? new List<string>();
            Rows = rows ?? new List<GenericConfigRow>();
        }
    }

    [Serializable]
    public sealed class GenericConfigRow
    {
        public List<string> Cells = new List<string>();
    }
}
