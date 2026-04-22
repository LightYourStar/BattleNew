using System.Collections.Generic;

namespace Game.Battle.Runtime.Entities.Hero
{
    /// <summary>
    /// 英雄定义注册表：运行时按 Id 查询 <see cref="HeroDef"/>。
    /// <para>
    /// 注册时机：在 <c>BattleBootstrap.EnterBattle(setupContext)</c> 的回调中，
    /// 由 Hotfix 层或游戏初始化层调用 <see cref="Register"/>。
    /// </para>
    /// </summary>
    public sealed class HeroDefRegistry
    {
        private readonly Dictionary<string, HeroDef> _defs = new();

        public void Register(HeroDef def)
        {
            _defs[def.Id] = def;
        }

        /// <summary>按 Id 查找英雄定义；未注册时返回 null。</summary>
        public HeroDef? Get(string id)
            => _defs.TryGetValue(id, out HeroDef? def) ? def : null;

        public IReadOnlyDictionary<string, HeroDef> All => _defs;
    }
}
