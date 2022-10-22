using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenKh.Common.TreeStruc
{
    public class TreeStrucWriter
    {
        private class Indenter
        {
            private int _depth = 0;

            public void Enter()
            {
                _depth++;
            }

            public void Leave()
            {
                _depth--;
            }

            public override string ToString() => new string(' ', _depth);
        }

        public static string GetString(object any)
        {
            var writer = new StringWriter();
            var indenter = new Indenter();
            GetString(writer, any, indenter);
            return writer.ToString();
        }

        private static void GetString(TextWriter writer, object any, Indenter indenter)
        {
            if (any is ICollection items)
            {
                foreach (var item in items)
                {
                    GetString(writer, item, indenter);
                }
            }
            else if (any != null)
            {
                var type = any.GetType();
                writer.WriteLine($"{indenter}{type.Name} " + "{");
                indenter.Enter();
                foreach (var prop in type.GetProperties())
                {
                    var propValue = prop.GetValue(any);
                    if (propValue != null)
                    {
                        if (propValue is ICollection)
                        {
                            var elementType = prop.PropertyType.GetInterfaces()
                                .Where(type => type == typeof(ICollection<>))
                                .Select(type => type.GetGenericArguments()[0])
                                .FirstOrDefault()
                                ?? (prop.PropertyType.IsArray
                                    ? prop.PropertyType.GetElementType()
                                    : null
                                );

                            if (IsValueTokenType(elementType))
                            {
                                writer.WriteLine($"{indenter}{prop.Name} {string.Join(" ", ((IEnumerable)propValue).Cast<object>().Select(it => GetValueToken(it)))}");
                            }
                            else
                            {
                                writer.WriteLine($"{indenter}{prop.Name} " + "{");
                                indenter.Enter();
                                GetString(writer, propValue, indenter);
                                indenter.Leave();
                                writer.WriteLine($"{indenter}" + "}");
                            }
                        }
                        else
                        {
                            writer.WriteLine($"{indenter}{prop.Name} {GetValueToken(propValue)}");
                        }
                    }
                }
                indenter.Leave();
                writer.WriteLine($"{indenter}" + "}");
            }
        }

        private static bool IsValueTokenType(Type elementType)
        {
            return elementType == typeof(string)
                || elementType == typeof(int)
                || elementType == typeof(uint)
                || elementType == typeof(float)
                || elementType == typeof(double)
                || elementType == typeof(short)
                || elementType == typeof(ushort)
                || elementType == typeof(byte)
                || elementType == typeof(sbyte)
                ;
        }

        private static string GetValueToken(object any)
        {
            var text = "" + any;
            if (text.Contains("\""))
            {
                return "\"" + text.Replace("\"", "\"\"") + "\"";
            }
            else
            {
                return text;
            }
        }
    }
}
