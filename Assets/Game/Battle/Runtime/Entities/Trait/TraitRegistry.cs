using System.Collections.Generic;
using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Entities.Trait
{
    /// <summary>
    /// 词条注册表：管理已激活词条的增删。
    /// </summary>
    public sealed class TraitRegistry
    {
        private readonly List<ITrait> _activeTraits = new();

        public IReadOnlyList<ITrait> ActiveTraits => _activeTraits;

        public void Equip(BattleContext context, ITrait trait)
        {
            _activeTraits.Add(trait);
            trait.OnEquip(context);
        }

        public void Unequip(BattleContext context, ITrait trait)
        {
            trait.OnUnequip(context);
            _activeTraits.Remove(trait);
        }
    }
}
