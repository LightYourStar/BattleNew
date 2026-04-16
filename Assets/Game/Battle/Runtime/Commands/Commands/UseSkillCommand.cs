namespace Game.Battle.Runtime.Commands.Commands
{
    public sealed class UseSkillCommand : IFrameCommand
    {
        public int Frame { get; }

        public CommandSource Source { get; }

        public string CasterId { get; }

        public string SkillId { get; }

        public string? TargetId { get; }

        public UseSkillCommand(int frame, CommandSource source, string casterId, string skillId, string? targetId)
        {
            Frame = frame;
            Source = source;
            CasterId = casterId;
            SkillId = skillId;
            TargetId = targetId;
        }
    }
}
