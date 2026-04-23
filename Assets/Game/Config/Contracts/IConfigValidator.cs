namespace Game.Config.Contracts
{
    /// <summary>
    /// 配置校验器契约：输入为“扁平行数据”，输出为错误列表；编辑器与 CI 可共用同一套规则。
    /// <para>HybridCLR 边界（稳定层）：契约不变；具体校验实现可逐步下沉到热更或独立校验程序集。</para>
    /// </summary>
    public interface IConfigValidator
    {
        /// <summary>对一批行执行校验，不通过时在 <see cref="ConfigValidationResult.Errors"/> 中追加项。</summary>
        ConfigValidationResult Validate(ConfigValidationInput input);
    }
}
