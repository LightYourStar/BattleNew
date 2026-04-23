# 新项目配置系统生成 Prompt（Unity + HybridCLR）
## 使用方式
把下面整段 Prompt 原样复制到新项目 AI 对话中使用。
建议先让 AI 只生成“配置系统骨架 + 最小可运行链路”，不要一开始生成全量业务表。
---
## Prompt 正文
你现在是资深 Unity 架构工程师，请为一个新的 `Unity + C# + HybridCLR` 项目设计并生成一套“可维护、可扩展、可校验、可回滚”的配置系统框架。
### 一、目标
请实现一套配置系统，满足：
1. 仍然支持 Excel 作为配置输入源（策划工作流不变）。
2. 支持生成 C# 配置类型与运行时可加载配置资源。
3. 运行时读取必须通过 `IConfigProvider`，禁止直接依赖全局单例（如 `Global.gApp`）。
4. 支持基础校验（主键、空键、重复键、类型错误、外键占位）。
5. 支持版本号与回滚预留能力。
6. 支持后续接入 Addressables 和 HybridCLR。
7. 先做最小可用方案，再扩展大表、多语言、热更新。
### 二、架构要求
请分层设计：
- `ConfigGenerator`：编辑器侧，负责 Excel 解析、代码生成、资源导出、校验报告。
- `ConfigRuntime`：运行时，负责加载、缓存、查询、版本切换。
- `ConfigContracts`：配置接口与通用类型，不依赖业务模块。
- `ConfigAdapters`：旧表结构到新结构的适配层（预留）。
### 三、目录结构要求
请生成以下目录结构：
```text
Assets/
  Game/
    Config/
      Contracts/
        IConfigProvider.cs
        IConfigTable.cs
        IConfigValidator.cs
        ConfigVersionInfo.cs
        ConfigLoadResult.cs
      Runtime/
        ConfigProvider.cs
        ConfigRegistry.cs
        ConfigCache.cs
        ConfigLocator.cs
      Generated/
        (放自动生成的配置类)
      Data/
        (放导出的配置资产)
      Adapters/
        OldPassDataAdapter.cs
        OldRoleDataAdapter.cs
      Editor/
        Excel/
          ExcelParser.cs
          ExcelSchema.cs
          ExcelValidationReport.cs
        Generator/
          ConfigCodeGenerator.cs
          ConfigAssetGenerator.cs
          ConfigGenerationPipeline.cs
        Validation/
          PrimaryKeyValidator.cs
          DuplicateKeyValidator.cs
          TypeValidator.cs
          ReferenceValidator.cs
        Window/
          ConfigGenerationWindow.cs
```
### 四、核心接口与类要求
请至少生成这些接口/类骨架（含职责注释）：
#### Contracts
- `IConfigProvider`
  - `GetTable<TTable>(string tableName)`
  - `TryGetTable<TTable>(string tableName, out TTable table)`
  - `Reload(ConfigVersionInfo versionInfo)`
- `IConfigTable<TKey, TItem>`
  - `bool TryGet(TKey key, out TItem item)`
  - `TItem Get(TKey key)`
  - `IReadOnlyList<TItem> GetAll()`
- `IConfigValidator`
  - `Validate(...) -> ExcelValidationReport`
#### Runtime
- `ConfigProvider`（实现 `IConfigProvider`）
- `ConfigRegistry`（表名与类型映射）
- `ConfigCache`（已加载配置缓存）
- `ConfigLocator`（按 key 查询）
#### Editor
- `ConfigGenerationWindow`（菜单入口）
- `ConfigGenerationPipeline`（串联解析 -> 校验 -> 生成）
- `ExcelParser`
- `ConfigCodeGenerator`
- `ConfigAssetGenerator`
- `ExcelValidationReport`
### 五、强约束（必须遵守）
1. 禁止全局单例耦合（不要生成 `Global.xxx` 风格访问）。
2. 禁止把生成器写成一个超大脚本；必须模块化。
3. 禁止弱语义字段承载复杂逻辑（不要用 PM1/PM2 风格字段）。
4. 生成器与运行时解耦，编辑器代码不能污染运行时程序集。
5. 运行时读取接口必须稳定，便于后续替换底层存储（SO/二进制/Addressables）。
6. 所有生成类要可读，命名清晰，注释说明来源表名。
### 六、最小可运行链路（第一阶段）
请先实现以下最小闭环：
1. 读取一个 Excel 文件（可先用 mock 数据入口占位）。
2. 生成 1 个配置表类（示例：`AttrConfigTable`）。
3. 生成对应配置资产（先 ScriptableObject）。
4. 运行时通过 `ConfigProvider` 加载并查询该表。
5. 演示 `Get/TryGet` 与错误处理。
### 七、校验要求（第一版必须有）
请实现这些基础校验：
- 主键不能为空。
- 主键不能重复。
- 字段类型必须匹配。
- 行级错误要能定位到：文件名 + Sheet + 行号 + 列名。
输出：`ExcelValidationReport`，可在 Editor Window 里查看。
### 八、版本与回滚（先预留）
请设计但不必完整实现：
- `ConfigVersionInfo`（版本号、来源、时间戳）
- `Reload` 入口
- 失败回滚到上一版本接口
说明清楚后续如何接 Addressables + 远端配置。
### 九、多语言与大表策略（先给结构，不要一次做完）
请先预留扩展点，不要第一版全实现：
- 多语言字段映射策略接口（例如 `ITextLocalizationResolver`）
- 大表分片策略接口（例如 `ITablePartitionStrategy`）
## 十、与 HybridCLR 的边界
请在代码注释中标注：
- 稳定层：`Contracts` + `Runtime` 核心接口
- 可热更层：表规则、表适配器、业务映射逻辑
原则：高频基础读取逻辑尽量保持稳定，不频繁热更。
### 十一、输出顺序要求
请按以下顺序输出：
1. 目录树
2. 各模块职责说明
3. 核心接口代码骨架
4. 最小闭环示例代码
5. 后续扩展路线（多语言、大表、远端热更）
不要直接输出大量业务配置类，只做骨架与一个示例表。
---
## 推荐追加指令（第二轮）
当第一版完成后，再追加这段：
```text
请在当前配置系统骨架上继续完善：
1. 新增 Addressables 加载器适配接口（IConfigAssetLoader）
2. 增加外键校验器（ReferenceValidator）
3. 增加配置生成摘要文件（json），用于 CI 对比
4. 增加一个简单的旧系统适配器示例（OldPassDataAdapter）
5. 保持运行时接口不变，不要引入全局单例
```
## 使用建议
- 第一轮只做骨架，不做复杂业务。
- 第二轮做校验和加载器抽象。
- 第三轮再接入旧表和战斗最小表集（`MainMission`、`PlayMode`、`Attr`）。