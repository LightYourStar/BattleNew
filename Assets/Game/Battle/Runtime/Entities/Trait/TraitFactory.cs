using System;
using System.Collections.Generic;

namespace Game.Battle.Runtime.Entities.Trait
{
    /// <summary>
    /// 词条工厂：按 TraitId 创建词条实例。
    /// <para>
    /// 设计思路：
    /// - 运行时通过 <see cref="Register"/> 注册"traitId → 工厂函数"映射。
    /// - 命令消费侧（BattleWorld.ApplySelectTrait）调用 <see cref="TryCreate"/> 创建具体词条实例，
    ///   再交给 TraitSystem.EquipTrait 装备——实现词条命令与词条实现的解耦。
    /// </para>
    /// <para>
    /// 扩展方式：
    /// - 普通词条：在 Bootstrap/游戏初始化时调用 Register 注册工厂函数。
    /// - 热更词条：在热更程序集 Awake 时注册，无需改动 Runtime 层。
    /// </para>
    /// </summary>
    public sealed class TraitFactory
    {
        private readonly Dictionary<string, Func<string, ITrait>> _registry = new();

        /// <summary>
        /// 注册一个词条工厂函数。
        /// </summary>
        /// <param name="traitId">词条唯一 Id（与 SelectTraitCommand.TraitId 对应）。</param>
        /// <param name="factory">工厂函数，参数为持有者 ownerId，返回词条实例。</param>
        public void Register(string traitId, Func<string, ITrait> factory)
        {
            _registry[traitId] = factory;
        }

        /// <summary>
        /// 尝试按 traitId 创建词条实例。
        /// </summary>
        /// <param name="traitId">词条 Id。</param>
        /// <param name="ownerId">词条持有者（通常是英雄 Id）。</param>
        /// <param name="trait">成功时返回词条实例；失败时为 null。</param>
        /// <returns>是否找到对应工厂。</returns>
        public bool TryCreate(string traitId, string ownerId, out ITrait? trait)
        {
            if (_registry.TryGetValue(traitId, out Func<string, ITrait>? factory))
            {
                trait = factory(ownerId);
                return true;
            }

            trait = null;
            return false;
        }

        /// <summary>当前已注册的词条 Id 列表（供调试/编辑器查看）。</summary>
        public IReadOnlyCollection<string> RegisteredIds => _registry.Keys;
    }
}
