namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>子弹弹道类型，对应 <see cref="BulletFactory"/> 的工厂方法。</summary>
    public enum BulletType
    {
        /// <summary>追踪弹：每帧朝目标当前位置飞行。</summary>
        Tracking,

        /// <summary>直线弹：发射时锁定方向，之后匀速飞行。</summary>
        Linear,

        /// <summary>抛物线弹：受重力影响的抛射物（炮弹、投掷物等）。</summary>
        Parabolic,
    }
}
