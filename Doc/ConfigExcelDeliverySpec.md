# 配置表交付规范（Config 工具链）

本文档用于统一策划导表与程序校验规则，适配当前配置工具链：

- 解析优先级：`xlsx > csv > mock`
- 外键规则来源：`Assets/Game/Config/Editor/Validation/ConfigReferenceRules.cs`
- 生成摘要：`Assets/Game/Config/Generated/generation-summary.json`
- 当前默认解析表：`Attr`、`Buff`（由 `ExcelParser.BuildDefaultBatch()` 决定）
- 类型矩阵文档：`Doc/ConfigFieldTypeMatrix.md`
- 管线版本：`pipelineVersion = "3"`（写入 `generation-summary.json`）

## 1. 文件与目录约定

- 样例目录固定为：`Assets/Game/Config/Editor/Excel/Samples/`
- 推荐交付格式：`.xlsx`
- 兼容格式：`.csv`（UTF-8）
- 支持两种组织方式：
  - 单表单文件（例如 `Attr.xlsx`）
  - 单功能单文件多 Sheet（文件名可为中文或任意业务名，如 `战斗系统配置.xlsx`）

## 2. Sheet 与表头约定

- 每个文件至少一张可读 Sheet。
- 在 `.xlsx` 中，解析器会扫描 Samples 目录下所有工作簿，按“Sheet 名 == 逻辑表名”精准匹配（例如找表 `Attr` 就找名为 `Attr` 的 Sheet）。
- 推荐直接把 Sheet 命名为逻辑表名（`Attr`、`Buff` 等），避免歧义。
- **第一行为表头（列名）**。
- **可选类型行**：若紧跟表头的下一行，每一列都能被解析为合法类型 token，则该行会被识别为类型行并从数据中移除；此时 **首条数据行在 Excel 中为第 3 行**（表头 + 类型行）。详见 `Doc/ConfigFieldTypeMatrix.md`。
- 若下一行无法被完整解析为类型行，则该行按**数据行**处理（兼容旧表：无类型行时仍按旧规则校验）。
- 列名请使用稳定英文标识，避免中文列名频繁改动。
- 若多个 xlsx 同时包含同名 Sheet，会按文件路径排序后的首个命中作为输入，建议避免重复 Sheet 名。

## 3. 字段填写规则

- 主键列默认使用 `Id`，不能为空、不能重复。
- **有类型行时**：各列类型以类型行为准，校验规则见 `Doc/ConfigFieldTypeMatrix.md`。
- **无类型行时（向后兼容）**：仍仅对列名 `Id`、`Value`、`AttrId`、`BuffId`、`SkillId` 做 `int` 解析校验。
- 数组列单元格为空串时按空数组处理。
- 空行会被忽略；建议不要在有效数据中间插空行。

## 4. 外键规则维护

- 外键规则在 `ConfigReferenceRules.cs` 中声明，例如：
  - `new ReferenceRule("Buff", "AttrId", "Attr", "Id")`
- 语义：`Buff.AttrId` 的值必须存在于 `Attr.Id`。
- 新增“引用字段”时，必须同步新增一条 `ReferenceRule`，否则不会被校验器检查。

## 5. CSV 额外约定

- 编码必须是 UTF-8（支持 BOM）。
- 以 `#` 开头的行会作为注释忽略。
- 带逗号的单元格请使用双引号包裹（例如 `"1,2,3"`、`"(1,2)"`）。
- 双引号转义使用 `""`（CSV 标准写法）。

## 6. 生成与验收流程

1. 策划更新 `Samples/*.xlsx`（可一个文件多个 Sheet）或 `Samples/<表名>.csv`。
2. 程序在 Unity 菜单执行：`Tools/Config/Generation Window`。
3. 点击 `Run Generate (Validate + Export)`。
4. 查看结果：
   - 校验错误列表（窗口）
   - `generation-summary.json`（CI 归档与对比）
   - `GeneratedSummary.txt`（快速人工确认）
5. 可选：执行类型自测菜单 `Tools/Config/Run Type Parser Self Test`。

## 7. 可选开关（开发向）

- `ConfigPipelineOptions.TreatUnknownTypesAsEnum`
  - `false`：未知类型直接报错。
  - `true`：未知类型按“合成枚举”处理，输出到 `Assets/Game/Config/Generated/Enums/*.gen.cs`。
- `ConfigPipelineOptions.UseHashString`
  - 当前为占位开关，预留给 string/string[] 哈希存储路径。
- `ConfigPipelineOptions.CompressColorIntoInt`
  - 当前为占位开关，预留给 Color <-> int 压缩序列化。

## 8. 常见问题排查

- **Q: 为什么读到了 mock？**  
  A: 没有找到同名 Sheet（xlsx）且没有 `Samples/<表名>.csv`，检查 `Samples` 目录、Sheet 命名以及当前 `BuildDefaultBatch()` 是否包含该逻辑表。

- **Q: 外键没报错是不是没生效？**  
  A: 先确认 `ConfigReferenceRules.cs` 已添加对应规则，并且来源表与目标表都在本次批次中。

- **Q: 我新增了 `Skill` 表但没有参与校验？**  
  A: 需要在 `ExcelParser.BuildDefaultBatch()` 里把 `Skill` 加入批次，否则工具链不会读取该表。

- **Q: CI 怎么判断是否通过？**  
  A: 读取 `generation-summary.json` 的 `validationSuccess` 与 `validationErrorCount`。

## 9. 推荐命名清单（可扩展）

- 主键：`Id`
- 外键：`<Target>Id`（如 `AttrId`、`BuffId`、`SkillId`）
- 文本键：`TextId`（预留给多语言解析器）

后续如接入 Addressables 与远端配置，可在不改变本交付规范的前提下，仅替换运行时加载器实现。
