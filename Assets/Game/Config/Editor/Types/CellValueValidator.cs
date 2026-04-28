#if UNITY_EDITOR
using System;
using System.Globalization;
using System.Linq;
namespace Game.Config.Editor.Types
{
    /// <summary>
    /// 单元格字符串校验：与 <see cref="FieldTypeParser"/> 输出的类型描述一致。
    /// 解析格式说明（Unity 常见写法）：
    /// <list type="bullet">
    /// <item><description>Vector2/3/4：可选括号，逗号分隔分量，如 <c>(1,2)</c> 或 <c>1, 2, 3</c>。</description></item>
    /// <item><description>Rect：<c>x,y,width,height</c> 四个 float，逗号分隔。</description></item>
    /// <item><description>Color：<c>r,g,b,a</c> 0~1 浮点；或 <c>#RRGGBB</c> / <c>#RRGGBBAA</c>。</description></item>
    /// <item><description>数组：逗号分隔元素；空串表示空数组；元素按对应标量规则解析。</description></item>
    /// </list>
    /// </summary>
    public static class CellValueValidator
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static bool TryValidate(string raw, FieldTypeDescriptor type, out string error)
        {
            error = null;
            var s = raw ?? string.Empty;
            var t = s.Trim();

            switch (type.Kind)
            {
                case BuiltinFieldKind.Int:
                    return TryParseInt(t, true, out error);
                case BuiltinFieldKind.Long:
                    return TryParseLong(t, true, out error);
                case BuiltinFieldKind.Float:
                    return TryParseFloat(t, true, out error);
                case BuiltinFieldKind.Double:
                    return TryParseDouble(t, true, out error);
                case BuiltinFieldKind.String:
                    return true;
                case BuiltinFieldKind.Vector2:
                    return TryParseVector2(t, out error);
                case BuiltinFieldKind.Vector3:
                    return TryParseVector3(t, out error);
                case BuiltinFieldKind.Vector4:
                    return TryParseVector4(t, out error);
                case BuiltinFieldKind.Rect:
                    return TryParseRect(t, out error);
                case BuiltinFieldKind.Color:
                    return TryParseColor(t, out error);
                case BuiltinFieldKind.IntArray:
                    return TryParseArray(t, TryParseIntElement, out error);
                case BuiltinFieldKind.LongArray:
                    return TryParseArray(t, TryParseLongElement, out error);
                case BuiltinFieldKind.FloatArray:
                    return TryParseArray(t, TryParseFloatElement, out error);
                case BuiltinFieldKind.DoubleArray:
                    return TryParseArray(t, TryParseDoubleElement, out error);
                case BuiltinFieldKind.StringArray:
                    return TryParseStringArray(t, out error);
                case BuiltinFieldKind.SyntheticEnum:
                    if (string.IsNullOrEmpty(t))
                    {
                        error = "Synthetic enum cell cannot be empty.";
                        return false;
                    }

                    return true;
                default:
                    error = $"Unsupported field kind: {type.Kind}";
                    return false;
            }
        }

        private delegate bool ElementParser(string token, out string err);

        private static bool TryParseArray(string t, ElementParser parseElement, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(t))
            {
                return true;
            }

            var parts = SplitCsvTopLevel(t);
            for (var i = 0; i < parts.Count; i++)
            {
                var p = parts[i].Trim();
                if (p.Length == 0)
                {
                    error = $"Array element {i} is empty.";
                    return false;
                }

                if (!parseElement(p, out var e))
                {
                    error = $"Array element {i}: {e}";
                    return false;
                }
            }

            return true;
        }

        private static bool TryParseStringArray(string t, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(t))
            {
                return true;
            }

            _ = SplitCsvTopLevel(t);
            return true;
        }

        /// <summary>顶层逗号切分（不做引号转义；string[] 元素内勿含逗号）。</summary>
        private static System.Collections.Generic.List<string> SplitCsvTopLevel(string t)
        {
            return t.Split(new[] { ',' }, StringSplitOptions.None).Select(p => p.Trim()).ToList();
        }

        private static bool TryParseInt(string t, bool required, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(t))
            {
                if (required)
                {
                    error = "int value required.";
                    return false;
                }

                return true;
            }

            if (!int.TryParse(t, NumberStyles.Integer, Inv, out _))
            {
                error = $"Invalid int: {t}";
                return false;
            }

            return true;
        }

        private static bool TryParseLong(string t, bool required, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(t))
            {
                if (required)
                {
                    error = "long value required.";
                    return false;
                }

                return true;
            }

            if (!long.TryParse(t, NumberStyles.Integer, Inv, out _))
            {
                error = $"Invalid long: {t}";
                return false;
            }

            return true;
        }

        private static bool TryParseFloat(string t, bool required, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(t))
            {
                if (required)
                {
                    error = "float value required.";
                    return false;
                }

                return true;
            }

            if (!float.TryParse(t, NumberStyles.Float | NumberStyles.AllowThousands, Inv, out _))
            {
                error = $"Invalid float: {t}";
                return false;
            }

            return true;
        }

        private static bool TryParseDouble(string t, bool required, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(t))
            {
                if (required)
                {
                    error = "double value required.";
                    return false;
                }

                return true;
            }

            if (!double.TryParse(t, NumberStyles.Float | NumberStyles.AllowThousands, Inv, out _))
            {
                error = $"Invalid double: {t}";
                return false;
            }

            return true;
        }

        private static bool TryParseIntElement(string t, out string e) => TryParseInt(t, true, out e);

        private static bool TryParseLongElement(string t, out string e) => TryParseLong(t, true, out e);

        private static bool TryParseFloatElement(string t, out string e) => TryParseFloat(t, true, out e);

        private static bool TryParseDoubleElement(string t, out string e) => TryParseDouble(t, true, out e);

        private static bool TryParseVector2(string t, out string error)
        {
            if (!TrySplitNumericList(t, 2, out var parts, out error))
            {
                return false;
            }

            if (!float.TryParse(parts[0], NumberStyles.Float, Inv, out _))
            {
                error = $"Invalid Vector2 x: {parts[0]}";
                return false;
            }

            if (!float.TryParse(parts[1], NumberStyles.Float, Inv, out _))
            {
                error = $"Invalid Vector2 y: {parts[1]}";
                return false;
            }

            return true;
        }

        private static bool TryParseVector3(string t, out string error)
        {
            if (!TrySplitNumericList(t, 3, out var parts, out error))
            {
                return false;
            }

            for (var i = 0; i < 3; i++)
            {
                if (!float.TryParse(parts[i], NumberStyles.Float, Inv, out _))
                {
                    error = $"Invalid Vector3 component[{i}]: {parts[i]}";
                    return false;
                }
            }

            return true;
        }

        private static bool TryParseVector4(string t, out string error)
        {
            if (!TrySplitNumericList(t, 4, out var parts, out error))
            {
                return false;
            }

            for (var i = 0; i < 4; i++)
            {
                if (!float.TryParse(parts[i], NumberStyles.Float, Inv, out _))
                {
                    error = $"Invalid float component: {parts[i]}";
                    return false;
                }
            }

            return true;
        }

        private static bool TryParseRect(string t, out string error)
        {
            if (!TrySplitNumericList(t, 4, out var parts, out error))
            {
                return false;
            }

            for (var i = 0; i < 4; i++)
            {
                if (!float.TryParse(parts[i], NumberStyles.Float, Inv, out _))
                {
                    error = $"Invalid rect component: {parts[i]}";
                    return false;
                }
            }

            return true;
        }

        private static bool TryParseColor(string t, out string error)
        {
            error = null;
            var s = t.Trim();
            if (string.IsNullOrEmpty(s))
            {
                error = "Color value required.";
                return false;
            }

            if (s.StartsWith("#", StringComparison.Ordinal))
            {
                return TryParseColorHex(s, out error);
            }

            if (!TrySplitNumericList(s, -1, out var parts, out error))
            {
                return false;
            }

            if (parts.Length != 3 && parts.Length != 4)
            {
                error = "Color expects 3 (RGB) or 4 (RGBA) float components in 0~1 range, or #RRGGBB/#RRGGBBAA.";
                return false;
            }

            foreach (var p in parts)
            {
                if (!float.TryParse(p, NumberStyles.Float, Inv, out var f))
                {
                    error = $"Invalid color component: {p}";
                    return false;
                }

                if (f < 0f || f > 1f)
                {
                    error = $"Color float components must be in [0,1]: {p}";
                    return false;
                }
            }

            return true;
        }

        private static bool TryParseColorHex(string s, out string error)
        {
            error = null;
            var hex = s.Substring(1);
            if (hex.Length != 6 && hex.Length != 8)
            {
                error = "Color hex must be #RRGGBB or #RRGGBBAA.";
                return false;
            }

            for (var i = 0; i < hex.Length; i++)
            {
                if (!Uri.IsHexDigit(hex[i]))
                {
                    error = $"Invalid hex digit in color: {hex[i]}";
                    return false;
                }
            }

            return true;
        }

        private static bool TrySplitNumericList(string t, int expectedCount, out string[] parts, out string error)
        {
            error = null;
            parts = null;
            var s = StripOptionalParens(t.Trim());
            if (string.IsNullOrWhiteSpace(s))
            {
                error = "List value required.";
                return false;
            }

            var raw = s.Split(',');
            parts = raw.Select(p => p.Trim()).Where(p => p.Length > 0).ToArray();
            if (expectedCount > 0 && parts.Length != expectedCount)
            {
                error = $"Expected {expectedCount} comma-separated values, got {parts.Length}.";
                return false;
            }

            if (parts.Length == 0)
            {
                error = "No numeric components found.";
                return false;
            }

            return true;
        }

        private static string StripOptionalParens(string s)
        {
            if (s.Length >= 2 && s[0] == '(' && s[s.Length - 1] == ')')
            {
                return s.Substring(1, s.Length - 2).Trim();
            }

            return s;
        }
    }
}
#endif
