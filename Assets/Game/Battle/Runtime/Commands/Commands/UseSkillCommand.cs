namespace Game.Battle.Runtime.Commands.Commands
{
    /// <summary>
    /// 释放技能命令：用于把“技能释放”从输入直驱中抽离出来，进入可录制/可回放链路。
    /// <para>
    /// 当前阶段：骨架占位。后续切片会把技能系统、目标选择、资源消耗等挂接到消费侧。
    /// </para>
    /// </summary>
    public sealed class UseSkillCommand : IFrameCommand
    {
        /// <inheritdoc />
        public int Frame { get; }

        /// <inheritdoc />
        public CommandSource Source { get; }

        /// <summary>施法者实体 Id（可能是英雄或怪物，取决于上层约定）。</summary>
        public string CasterId { get; }

        /// <summary>技能配置 Id。</summary>
        public string SkillId { get; }

        /// <summary>可选锁定目标 Id（无目标技能可为 null）。</summary>
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
