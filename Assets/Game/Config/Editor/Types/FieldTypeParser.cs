#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Game.Config.Editor.Types
{
    /// <summary>
    /// 类型行解析入口：仅接受标量关键字与标准 <c>T[]</c> 数组写法（大小写不敏感）。
    /// </summary>
    public static class FieldTypeParser
    {
        private static readonly Regex SyntheticEnumNameRegex = new Regex(
            @"^[_a-zA-Z][_a-zA-Z0-9]*$",
            RegexOptions.Compiled);

        private static readonly Dictionary<string, BuiltinFieldKind> Scalars =
            new Dictionary<string, BuiltinFieldKind>(StringComparer.OrdinalIgnoreCase)
            {
                ["int"] = BuiltinFieldKind.Int,
                ["float"] = BuiltinFieldKind.Float,
                ["double"] = BuiltinFieldKind.Double,
                ["long"] = BuiltinFieldKind.Long,
                ["string"] = BuiltinFieldKind.String,
                ["vector2"] = BuiltinFieldKind.Vector2,
                ["vector3"] = BuiltinFieldKind.Vector3,
                ["vector4"] = BuiltinFieldKind.Vector4,
                ["rect"] = BuiltinFieldKind.Rect,
                ["color"] = BuiltinFieldKind.Color,
            };

        private static readonly HashSet<string> ArrayElementScalars =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "int",
                "long",
                "float",
                "double",
                "string"
            };

        /// <summary>解析类型行 token；失败时 <paramref name="error"/> 非空。</summary>
        public static bool TryParseFieldType(string rawToken, bool treatUnknownTypesAsEnum, out FieldTypeDescriptor descriptor, out string error)
        {
            descriptor = default;
            error = null;

            if (string.IsNullOrWhiteSpace(rawToken))
            {
                error = "Empty type token.";
                return false;
            }

            var token = rawToken.Trim();

            if (token.EndsWith("[]", StringComparison.Ordinal))
            {
                var element = token.Substring(0, token.Length - 2).Trim();
                if (!ArrayElementScalars.Contains(element))
                {
                    error = $"Unsupported array element type '{element}'. Only int[], long[], float[], double[], string[] are allowed.";
                    return false;
                }

                descriptor = MapArray(element);
                return true;
            }

            if (Scalars.TryGetValue(token, out var scalar))
            {
                descriptor = new FieldTypeDescriptor(scalar);
                return true;
            }

            if (treatUnknownTypesAsEnum && SyntheticEnumNameRegex.IsMatch(token))
            {
                descriptor = new FieldTypeDescriptor(BuiltinFieldKind.SyntheticEnum, token);
                return true;
            }

            error = treatUnknownTypesAsEnum
                ? $"Unknown type '{token}' and not a valid enum type name."
                : $"Unknown type '{token}'. Enable TreatUnknownTypesAsEnum to treat identifiers as enums.";
            return false;
        }

        private static FieldTypeDescriptor MapArray(string elementLower)
        {
            switch (elementLower.ToLowerInvariant())
            {
                case "int":
                    return new FieldTypeDescriptor(BuiltinFieldKind.IntArray);
                case "long":
                    return new FieldTypeDescriptor(BuiltinFieldKind.LongArray);
                case "float":
                    return new FieldTypeDescriptor(BuiltinFieldKind.FloatArray);
                case "double":
                    return new FieldTypeDescriptor(BuiltinFieldKind.DoubleArray);
                case "string":
                    return new FieldTypeDescriptor(BuiltinFieldKind.StringArray);
                default:
                    return new FieldTypeDescriptor(BuiltinFieldKind.None);
            }
        }
    }
}
#endif
