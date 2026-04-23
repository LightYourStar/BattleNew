#if UNITY_EDITOR
using Game.Config.Contracts;

namespace Game.Config.Editor.Validation
{
    /// <summary>
    /// 外键占位校验器：第一版不检查跨表引用；后续可加载目标表主键集合，校验本表引用列是否都存在。
    /// </summary>
    public sealed class ReferenceValidator : IConfigValidator
    {
        /// <inheritdoc />
        public ConfigValidationResult Validate(ConfigValidationInput input)
        {
            return new ConfigValidationResult();
        }
    }
}
#endif
