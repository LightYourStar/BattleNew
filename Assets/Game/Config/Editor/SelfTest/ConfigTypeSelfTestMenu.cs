#if UNITY_EDITOR
using System;
using System.Text;
using Game.Config.Editor.Types;
using UnityEditor;

namespace Game.Config.Editor.SelfTest
{
    /// <summary>类型解析与单元格校验自测（菜单一键运行）。</summary>
    public static class ConfigTypeSelfTestMenu
    {
        [MenuItem("Tools/Config/Run Type Parser Self Test")]
        private static void Run()
        {
            var sb = new StringBuilder();
            var failed = false;

            void AssertTrue(bool ok, string msg)
            {
                if (!ok)
                {
                    failed = true;
                    sb.AppendLine("FAIL: " + msg);
                }
            }

            void AssertFalse(bool bad, string msg)
            {
                AssertTrue(!bad, msg);
            }

            // --- ParseFieldType ---
            AssertTrue(FieldTypeParser.TryParseFieldType("int", false, out var dInt, out _), "int");
            AssertTrue(dInt.Kind == BuiltinFieldKind.Int, "int kind");
            AssertTrue(FieldTypeParser.TryParseFieldType("INT[]", false, out var dIntArr, out _), "int[] case");
            AssertTrue(dIntArr.Kind == BuiltinFieldKind.IntArray, "int[] kind");
            AssertFalse(FieldTypeParser.TryParseFieldType("ints", false, out _, out _), "reject alias ints");
            AssertFalse(FieldTypeParser.TryParseFieldType("int[ ]", false, out _, out _), "reject spaced int[]");
            AssertFalse(FieldTypeParser.TryParseFieldType("byte[]", false, out _, out _), "reject byte[]");
            AssertFalse(FieldTypeParser.TryParseFieldType("MyEnum", false, out _, out _), "unknown without flag");
            AssertTrue(FieldTypeParser.TryParseFieldType("MyEnum", true, out var dEnum, out _), "unknown with flag");
            AssertTrue(dEnum.IsSyntheticEnum, "synthetic enum kind");

            // --- Cell scalars ---
            AssertTrue(CellValueValidator.TryValidate("1", dInt, out _), "int ok");
            AssertFalse(CellValueValidator.TryValidate("x", dInt, out _), "int bad");

            FieldTypeParser.TryParseFieldType("float", false, out var dFloat, out _);
            AssertTrue(CellValueValidator.TryValidate("1.5", dFloat, out _), "float ok");
            AssertFalse(CellValueValidator.TryValidate("nanx", dFloat, out _), "float bad");

            FieldTypeParser.TryParseFieldType("double", false, out var dDouble, out _);
            AssertTrue(CellValueValidator.TryValidate("2.25", dDouble, out _), "double ok");

            FieldTypeParser.TryParseFieldType("long", false, out var dLong, out _);
            AssertTrue(CellValueValidator.TryValidate("9223372036854775807", dLong, out _), "long ok");

            FieldTypeParser.TryParseFieldType("string", false, out var dStr, out _);
            AssertTrue(CellValueValidator.TryValidate("", dStr, out _), "string empty ok");

            FieldTypeParser.TryParseFieldType("vector2", false, out var dV2, out _);
            AssertTrue(CellValueValidator.TryValidate("(1,2)", dV2, out _), "vec2 paren ok");
            AssertTrue(CellValueValidator.TryValidate("1, 2", dV2, out _), "vec2 plain ok");
            AssertFalse(CellValueValidator.TryValidate("1", dV2, out _), "vec2 bad count");

            FieldTypeParser.TryParseFieldType("vector3", false, out var dV3, out _);
            AssertTrue(CellValueValidator.TryValidate("0,0,1", dV3, out _), "vec3 ok");

            FieldTypeParser.TryParseFieldType("vector4", false, out var dV4, out _);
            AssertTrue(CellValueValidator.TryValidate("1,2,3,4", dV4, out _), "vec4 ok");

            FieldTypeParser.TryParseFieldType("rect", false, out var dRect, out _);
            AssertTrue(CellValueValidator.TryValidate("0,0,100,50", dRect, out _), "rect ok");
            AssertFalse(CellValueValidator.TryValidate("0,0,100", dRect, out _), "rect bad");

            FieldTypeParser.TryParseFieldType("color", false, out var dColor, out _);
            AssertTrue(CellValueValidator.TryValidate("1,0,0,1", dColor, out _), "color rgba ok");
            AssertTrue(CellValueValidator.TryValidate("#FF0000", dColor, out _), "color hex ok");
            AssertFalse(CellValueValidator.TryValidate("2,0,0,1", dColor, out _), "color float out of range");

            FieldTypeParser.TryParseFieldType("int[]", false, out var dIA, out _);
            AssertTrue(CellValueValidator.TryValidate("", dIA, out _), "int[] empty ok");
            AssertTrue(CellValueValidator.TryValidate("1,2,3", dIA, out _), "int[] ok");
            AssertFalse(CellValueValidator.TryValidate("1,x", dIA, out _), "int[] bad element");

            FieldTypeParser.TryParseFieldType("string[]", false, out var dSA, out _);
            AssertTrue(CellValueValidator.TryValidate("a,b", dSA, out _), "string[] ok");

            var msg = failed ? "TYPE SELF TEST FAILED\n" + sb : "TYPE SELF TEST OK";
            if (failed)
            {
                UnityEngine.Debug.LogError(msg);
            }
            else
            {
                UnityEngine.Debug.Log(msg);
            }
        }
    }
}
#endif
