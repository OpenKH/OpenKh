using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;

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
                    .Select(expression => $"{GetMemberName(expression)}={expression.Compile().Invoke(item)}")
                    .ToList()
            };
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
            list.Add($"{name}={value}");
            return this;
        }
    }
}
