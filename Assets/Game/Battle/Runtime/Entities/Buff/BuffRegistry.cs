using System.Collections.Generic;
using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Entities.Buff
{
    /// <summary>
    /// Buff 注册表：管理运行时 Buff 的增删查与 Tick。
    /// </summary>
    public sealed class BuffRegistry
    {
        private readonly List<IBuff> _activeBuffs = new();
        private readonly List<IBuff> _pendingRemove = new();

        public IReadOnlyList<IBuff> ActiveBuffs => _activeBuffs;

        public void Add(BattleContext context, IBuff buff)
        {
            _activeBuffs.Add(buff);
            buff.OnAdd(context);
            context.DebugTraceService.TraceBuffChange(buff.OwnerId, buff.BuffId, "Add");
        }

        public void Remove(BattleContext context, IBuff buff)
        {
            buff.OnRemove(context);
            _activeBuffs.Remove(buff);
            context.DebugTraceService.TraceBuffChange(buff.OwnerId, buff.BuffId, "Remove");
        }

        public void Tick(BattleContext context, float deltaTime)
        {
            _pendingRemove.Clear();
            for (int i = 0; i < _activeBuffs.Count; i++)
            {
                IBuff buff = _activeBuffs[i];
                buff.OnTick(context, deltaTime);
                if (buff.IsExpired)
                {
                    _pendingRemove.Add(buff);
                }
            }

            for (int i = 0; i < _pendingRemove.Count; i++)
            {
                Remove(context, _pendingRemove[i]);
            }
        }
    }
}
