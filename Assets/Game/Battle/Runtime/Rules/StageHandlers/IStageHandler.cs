using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Rules.StageHandlers
{
    /// <summary>
    /// 关卡流程处理器：表达“这一关怎么推进”的条件与副作用挂载点。
    /// </summary>
    public interface IStageHandler
    {
        /// <summary>关卡是否已达成清场/通关条件（由具体实现定义）。</summary>
        bool IsStageCleared { get; }

        /// <summary>每逻辑帧更新：用于推进波次、触发阶段事件、刷怪等。</summary>
        void Tick(BattleContext context, float deltaTime);
    }
}
