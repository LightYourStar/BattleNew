using UnityEngine;

namespace Game.Battle.Runtime.Commands.Commands
{
    public sealed class MoveCommand : IFrameCommand
    {
        public int Frame { get; }

        public CommandSource Source { get; }

        public string HeroId { get; }

        public Vector3 Direction { get; }

        public MoveCommand(int frame, CommandSource source, string heroId, Vector3 direction)
        {
            Frame = frame;
            Source = source;
            HeroId = heroId;
            Direction = direction;
        }
    }
}
