namespace Game.Battle.Runtime.Commands.Commands
{
    /// <summary>
    /// 选择词条命令：用于把“词条选择”纳入命令系统，从而支持回放与网络对齐。
    /// <para>
    /// 当前阶段：骨架占位。词条系统（TraitSystem）后续切片才会真正消费该命令。
    /// </para>
    /// </summary>
    public sealed class SelectTraitCommand : IFrameCommand
    {
        /// <inheritdoc />
        public int Frame { get; }

        /// <inheritdoc />
        public CommandSource Source { get; }

        /// <summary>发起选择的英雄 Id（也可能是玩家主体 Id，取决于上层约定）。</summary>
        public string HeroId { get; }

        /// <summary>被选择的词条 Id。</summary>
        public string TraitId { get; }

        public SelectTraitCommand(int frame, CommandSource source, string heroId, string traitId)
        {
            Frame = frame;
            Source = source;
            HeroId = heroId;
            TraitId = traitId;
        }
    }
}
