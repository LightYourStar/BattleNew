# 配置表类型行 → C# 类型对照（方案 A）

本表描述 **类型行（第二行）** 中允许的 token、对应 C# 类型、以及单元格字符串格式约定。  
数组 **仅支持标准 `T[]` 写法**（大小写不敏感），不接受 `ints`、`raw`、`[int]` 等历史别名（若需兼容旧表，请在导入前单独做归一化预处理）。

## 标量

| 类型行 token | C# 类型 | 单元格格式 |
|--------------|---------|------------|
| `int` | `int` | 十进制整数 |
| `long` | `long` | 十进制整数 |
| `float` | `float` | `InvariantCulture` 浮点 |
| `double` | `double` | `InvariantCulture` 浮点 |
| `string` | `string` | 任意文本；空串合法 |
| `vector2` | `UnityEngine.Vector2` | `x,y` 或 `(x,y)` |
| `vector3` | `UnityEngine.Vector3` | `x,y,z` 或 `(x,y,z)` |
| `vector4` | `UnityEngine.Vector4` | `x,y,z,w` 或 `(x,y,z,w)` |
| `rect` | `UnityEngine.Rect` | `x,y,width,height`（四个 float，逗号分隔） |
| `color` | `UnityEngine.Color` | `r,g,b,a`（0~1 浮点，3 或 4 分量）或 `#RRGGBB` / `#RRGGBBAA` |

## 数组（仅 `T[]`）

| 类型行 token | C# 类型 | 单元格格式 |
|--------------|---------|------------|
| `int[]` | `int[]` | 逗号分隔；**空串 = 空数组**；元素不可空 |
| `long[]` | `long[]` | 同上 |
| `float[]` | `float[]` | 同上 |
| `double[]` | `double[]` | 同上 |
| `string[]` | `string[]` | 逗号分隔；**元素内勿含逗号**（当前读取器不做引号转义） |

## 合成枚举（可选）

| 条件 | 行为 |
|------|------|
| `ConfigPipelineOptions.TreatUnknownTypesAsEnum == true` 且类型 token 为合法标识符 | 视为枚举类型名；收集该列非空取值生成 `Assets/Game/Config/Generated/Enums/<Name>.gen.cs` |
| 否则 | 类型无法识别时报错 |

## 可选开关（占位）

| 开关 | 说明 |
|------|------|
| `ConfigPipelineOptions.UseHashString` | 预留：`string` / `string[]` 哈希存储路径 |
| `ConfigPipelineOptions.CompressColorIntoInt` | 预留：`Color` 与 `int` 互转 |

## 解析入口（代码）

- 类型行 token：`Game.Config.Editor.Types.FieldTypeParser.TryParseFieldType`
- 单元格校验：`Game.Config.Editor.Types.CellValueValidator.TryValidate`

## 自测

Unity 菜单：`Tools/Config/Run Type Parser Self Test`
