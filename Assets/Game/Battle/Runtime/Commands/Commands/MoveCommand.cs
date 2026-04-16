using UnityEngine;

namespace Game.Battle.Runtime.Commands.Commands
{
    /// <summary>
    /// 移动命令：表达“某个英雄在本帧希望朝某方向移动”的意图。
    /// <para>
    /// 说明：
    /// - Direction 通常是未归一化的输入向量（例如 WASD 合成），具体归一化与速度在消费侧处理。
    /// - 这是切片 2（命令替换直驱）的关键基础命令之一。
    /// </para>
    /// </summary>
    public sealed class MoveCommand : IFrameCommand
    {
        /// <inheritdoc />
        public int Frame { get; }

        /// <inheritdoc />
        public CommandSource Source { get; }

        /// <summary>要移动的英雄实体 Id。</summary>
        public string HeroId { get; }

        /// <summary>移动方向（世界空间）。</summary>
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
