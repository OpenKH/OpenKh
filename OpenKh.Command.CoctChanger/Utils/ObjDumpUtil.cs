using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;

namespace OpenKh.Command.CoctChanger.Utils
{
    class ObjDumpUtil
    {
        private Type type;
        private List<string> list;

        public override string ToString() => $"[{type.Name}] {string.Join("; ", list)}";

        public static ObjDumpUtil FormatObj<T>(
            T item,
            params Expression<Func<T, object>>[] expressions
        )
        {
            return new ObjDumpUtil
            {
                type = typeof(T),
                list = expressions
                    .Select(expression => $"{GetMemberName(expression)}={FormatValue(expression.Compile().Invoke(item))}")
                    .ToList()
            };
        }

        private static string FormatValue(object value)
        {
            if (value is Plane plane)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "{{Normal:<{0}, {1}, {2}> D:{3}}}",
                    plane.Normal.X,
                    plane.Normal.Y,
                    plane.Normal.Z,
                    plane.D
                );
            }

            if (value is Vector4 vector4)
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "<{0}, {1}, {2}, {3}>",
                    vector4.X,
                    vector4.Y,
                    vector4.Z,
                    vector4.W
                );
            }

            return value?.ToString() ?? "null";
        }

        private static string GetMemberName(LambdaExpression expression)
        {
            if (expression.Body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            {
                if (unary.Operand is MemberExpression binary)
                {
                    return binary.Member.Name;
                }
            }
            else if (expression.Body is MemberExpression binary)
            {
                return binary.Member.Name;
            }
            return "?";
        }

        public ObjDumpUtil Add(string name, object value)
        {
            list.Add($"{name}={FormatValue(value)}");
            return this;
        }
    }
}
