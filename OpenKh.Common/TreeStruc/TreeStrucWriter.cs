using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
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

            public override string ToString() => new string(' ', 2 * _depth);
        }

        public static string GetString(object any)
        {
            var writer = new StringWriter();
            var indenter = new Indenter();
            GetString(writer, any, indenter);
            return writer.ToString();
        }

        private interface ICodec
        {

        }

        private class EntryCodec : ICodec
        {
            internal string nodeName;
        }

        private class SimpleCollectionCodec : ICodec
        {
            internal string nodeName;
        }

        private class ObjectCollectionCodec : ICodec
        {
            internal string nodeName;
        }

        private class UnnamedObjectCollectionCodec : ICodec
        {

        }

        private class ObjectCodec : ICodec
        {
            internal string nodeName;
            internal Dictionary<string, PropertyInfo> props;
        }

        private static readonly Dictionary<string, ICodec> _cache = new Dictionary<string, ICodec>();
        private static readonly object _lock = new object();

        private static ICodec GetCodec(Type baseType, string nodeName)
        {
            lock (_lock)
            {
                var cacheKey = baseType.FullName + "|" + nodeName;
                if (!_cache.TryGetValue(cacheKey, out ICodec codec))
                {
                    codec = _cache[cacheKey] = GetCodecCore(baseType, nodeName);
                }
                return codec;
            }
        }

        private static ICodec GetCodecCore(Type baseType, string nodeName)
        {
            if (typeof(ICollection).IsAssignableFrom(baseType))
            {
                var elementType = baseType.GetInterfaces()
                    .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ICollection<>))
                    .Select(type => type.GetGenericArguments()[0])
                    .FirstOrDefault()
                    ?? (baseType.IsArray
                        ? baseType.GetElementType()
                        : null
                    );

                if (elementType != null)
                {
                    if (nodeName != null)
                    {
                        if (IsValueTokenType(elementType))
                        {
                            return new SimpleCollectionCodec { nodeName = nodeName, };
                        }
                        else
                        {
                            return new ObjectCollectionCodec { nodeName = nodeName, };
                        }
                    }
                    else
                    {
                        return new UnnamedObjectCollectionCodec();
                    }
                }
                else
                {
                    return null;
                }
            }
            else if (IsValueTokenType(baseType))
            {
                if (nodeName != null)
                {
                    return new EntryCodec { nodeName = nodeName, };
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new ObjectCodec
                {
                    nodeName = baseType.Name,
                    props = baseType.GetProperties()
                        .ToDictionary(it => it.Name, it => it),
                };
            }
        }

        private static void GetString(TextWriter writer, object any, Indenter indenter)
        {
            if (any != null)
            {
                var type = any.GetType();
                var codec = GetCodec(type, null);

                void WriteEntry(EntryCodec it, object target)
                {
                    writer.WriteLine($"{indenter}{it.nodeName} {GetValueToken(target)}");
                }

                void WriteArray(SimpleCollectionCodec it, IEnumerable target)
                {
                    var values = string.Join(" ", target.Cast<object>().Select(it => GetValueToken(it)));
                    writer.WriteLine($"{indenter}{it.nodeName} {values}");
                }

                void WriteObjects(ObjectCollectionCodec it, IEnumerable<object> target)
                {
                    writer.WriteLine($"{indenter}{it.nodeName}");
                    writer.WriteLine($"{indenter}" + "{");
                    indenter.Enter();
                    foreach (var item in target)
                    {
                        if (GetCodec(item.GetType(), null) is ObjectCodec objectCodec)
                        {
                            WriteObject(objectCodec, item);
                        }
                    }
                    indenter.Leave();
                    writer.WriteLine($"{indenter}" + "}");
                }

                void WriteUnnamedObjects(UnnamedObjectCollectionCodec it, IEnumerable<object> target)
                {
                    foreach (var item in target)
                    {
                        if (GetCodec(item.GetType(), null) is ObjectCodec objectCodec)
                        {
                            WriteObject(objectCodec, item);
                        }
                    }
                }

                void WriteObject(ObjectCodec it, object target)
                {
                    writer.WriteLine($"{indenter}{it.nodeName}");
                    writer.WriteLine($"{indenter}" + "{");
                    indenter.Enter();
                    foreach (var pair in it.props)
                    {
                        var subCodec = GetCodec(pair.Value.PropertyType, pair.Key);
                        var propValue = pair.Value.GetValue(target);

                        if (propValue != null)
                        {
                            if (subCodec is SimpleCollectionCodec simpleCollectionCodec)
                            {
                                WriteArray(simpleCollectionCodec, (IEnumerable)propValue);
                            }
                            else if (subCodec is ObjectCollectionCodec objectCollectionCodec)
                            {
                                WriteObjects(objectCollectionCodec, (IEnumerable<object>)propValue);
                            }
                            else if (subCodec is ObjectCodec objectCodec)
                            {
                                WriteObject(objectCodec, propValue);
                            }
                            else if (subCodec is EntryCodec entryCodec)
                            {
                                WriteEntry(entryCodec, propValue);
                            }
                        }
                    }
                    indenter.Leave();
                    writer.WriteLine($"{indenter}" + "}");
                }

                if (codec is SimpleCollectionCodec simpleCollectionCodec)
                {
                    WriteArray(simpleCollectionCodec, (IEnumerable<object>)any);
                }
                else if (codec is ObjectCollectionCodec objectCollectionCodec)
                {
                    WriteObjects(objectCollectionCodec, (IEnumerable<object>)any);
                }
                else if (codec is UnnamedObjectCollectionCodec unnamedObjectCollectionCodec)
                {
                    WriteUnnamedObjects(unnamedObjectCollectionCodec, (IEnumerable<object>)any);
                }
                else if (codec is ObjectCodec objectCodec)
                {
                    WriteObject(objectCodec, any);
                }
                else if (codec is EntryCodec entryCodec)
                {
                    WriteEntry(entryCodec, any);
                }
            }
        }

        private static void GetStringOld(TextWriter writer, object any, Indenter indenter)
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
                || elementType.IsEnum
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
