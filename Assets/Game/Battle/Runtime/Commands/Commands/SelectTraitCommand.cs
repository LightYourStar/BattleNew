namespace Game.Battle.Runtime.Commands.Commands
{
    public sealed class SelectTraitCommand : IFrameCommand
    {
        public int Frame { get; }

        public CommandSource Source { get; }

        public string HeroId { get; }

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
