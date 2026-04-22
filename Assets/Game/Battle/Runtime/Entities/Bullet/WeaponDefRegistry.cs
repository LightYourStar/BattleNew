using System.Collections.Generic;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 武器定义注册表：运行时按 Id 查询 <see cref="WeaponDef"/>。
    /// <para>
    /// 注册时机：在 <c>BattleBootstrap.EnterBattle(setupContext)</c> 的回调中，
    /// 由 Hotfix 层或游戏初始化层调用 <see cref="Register"/>。
    /// </para>
    /// </summary>
    public sealed class WeaponDefRegistry
    {
        private readonly Dictionary<string, WeaponDef> _defs = new();

        public void Register(WeaponDef def)
        {
            _defs[def.Id] = def;
        }

        /// <summary>按 Id 查找武器定义；未注册时返回 null。</summary>
        public WeaponDef? Get(string id)
            => _defs.TryGetValue(id, out WeaponDef? def) ? def : null;

        public IReadOnlyDictionary<string, WeaponDef> All => _defs;
    }
}
