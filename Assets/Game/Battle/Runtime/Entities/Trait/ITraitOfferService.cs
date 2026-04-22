using System.Collections.Generic;
using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Entities.Trait
{
    /// <summary>
    /// 词条 Offer 服务：为局内升级界面生成一份经过确定性随机的词条候选列表。
    /// <para>
    /// 设计约束：
    /// <list type="bullet">
    ///   <item>必须使用 <see cref="BattleContext.Rng"/> 进行随机，确保可回放。</item>
    ///   <item>只负责"出什么"，不负责"玩家选了什么"；后者通过 <c>SelectTraitCommand</c> 进入命令链。</item>
    /// </list>
    /// </para>
    /// </summary>
    public interface ITraitOfferService
    {
        /// <summary>
        /// 生成一次词条升级 Offer。
        /// </summary>
        /// <param name="context">当前战斗上下文（用于取 RNG、查询已装备词条等）。</param>
        /// <param name="count">每次展示的候选数量（通常 3）。</param>
        /// <returns>
        /// 词条 Id 列表（长度 ≤ count；池中词条不足时自动截短）。
        /// 调用方（UI 层）将此列表展示给玩家，玩家操作通过 <c>SelectTraitCommand</c> 返回。
        /// </returns>
        IReadOnlyList<string> GenerateOffer(BattleContext context, int count = 3);
    }
}
