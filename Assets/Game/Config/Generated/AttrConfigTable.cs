using System;
using System.Collections.Generic;
using Game.Config.Contracts;
using UnityEngine;

namespace Game.Config.Generated
{
    /// <summary>
    /// 示例表：来源表 Attr；运行时以 ScriptableObject 资产形式存在，实现 <see cref="IConfigTable{TKey,TItem}"/> 供查询。
    /// <para>HybridCLR 边界（可热更倾向）：典型生成表类，可置于热更 DLL；查询接口在 Contracts 保持稳定。</para>
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Config/AttrConfigTable", fileName = "AttrConfigTable")]
    public sealed class AttrConfigTable : ScriptableObject, IConfigTable<int, AttrConfigItem>
    {
        [SerializeField] private List<AttrConfigItem> items = new List<AttrConfigItem>();
        private Dictionary<int, AttrConfigItem> _index;

        /// <inheritdoc />
        public bool TryGet(int key, out AttrConfigItem item)
        {
            EnsureIndex();
            return _index.TryGetValue(key, out item);
        }

        /// <inheritdoc />
        public AttrConfigItem Get(int key)
        {
            if (TryGet(key, out var item))
            {
                return item;
            }

            throw new KeyNotFoundException($"AttrConfig key not found: {key}");
        }

        /// <inheritdoc />
        public IReadOnlyList<AttrConfigItem> GetAll()
        {
            return items;
        }

        /// <summary>
        /// 编辑器生成管线写入行数据后调用：替换序列化列表并丢弃旧索引，下次查询会重建字典。
        /// </summary>
        public void ResetItems(List<AttrConfigItem> source)
        {
            items = source ?? new List<AttrConfigItem>();
            _index = null;
        }

        /// <summary>懒构建主键字典：首次查询时把 List 摊平为 Dictionary，后续 O(1) 查行。</summary>
        private void EnsureIndex()
        {
            if (_index != null)
            {
                return;
            }

            _index = new Dictionary<int, AttrConfigItem>();
            foreach (var item in items)
            {
                _index[item.Id] = item;
            }
        }
    }

    /// <summary>Attr 表一行：与策划列 Id / Name / Value 对应。</summary>
    [Serializable]
    public sealed class AttrConfigItem
    {
        public int Id;
        public string Name;
        public int Value;
    }
}
