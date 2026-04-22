using UnityEngine;

namespace Game.Battle.Runtime.Entities.Bullet
{
    /// <summary>
    /// 武器枪口挂点（局部坐标）：描述相对于英雄中心的子弹生成位置与发射方向。
    /// <para>
    /// <b>坐标约定</b>：局部 forward 为 (0, 0, 1)（Unity 默认 forward）。
    /// <see cref="WeaponFireService"/> 在发射时将局部坐标旋转到英雄当前朝向的世界空间。
    /// </para>
    /// <para>
    /// <b>多枪口示例</b>：双枪武器可配置两个 FireSocket，左枪口 LocalOffset = (-0.3, 0, 0.5)，
    /// 右枪口 LocalOffset = (0.3, 0, 0.5)，每次开火产生两颗子弹。
    /// </para>
    /// </summary>
    public sealed class FireSocket
    {
        /// <summary>相对于英雄中心的局部位置偏移（英雄面朝 (0,0,1) 时的坐标系）。</summary>
        public Vector3 LocalOffset { get; set; } = Vector3.zero;

        /// <summary>
        /// 局部发射方向（归一化）。
        /// 追踪弹忽略此字段（始终朝向目标）；直线弹/抛物线弹使用此字段。
        /// 默认 Vector3.forward 即英雄正前方。
        /// </summary>
        public Vector3 LocalForward { get; set; } = Vector3.forward;

        /// <summary>正前方单枪口（最常用，零偏移、正前方发射）。</summary>
        public static readonly FireSocket Default = new();

        /// <summary>左侧偏移枪口（双枪左）。</summary>
        public static FireSocket Left(float sideOffset = 0.3f, float forwardOffset = 0.5f)
            => new() { LocalOffset = new Vector3(-sideOffset, 0f, forwardOffset) };

        /// <summary>右侧偏移枪口（双枪右）。</summary>
        public static FireSocket Right(float sideOffset = 0.3f, float forwardOffset = 0.5f)
            => new() { LocalOffset = new Vector3(sideOffset, 0f, forwardOffset) };
    }
}
