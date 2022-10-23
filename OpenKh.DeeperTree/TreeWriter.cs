using Antlr4.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using static TreeScriptParser;

namespace OpenKh.DeeperTree
{
    public class TreeWriter
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

        public static string Serialize(object any)
        {
            var writer = new StringWriter();
            var indenter = new Indenter();
            Serialize(writer, any, indenter);
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

        private static void Serialize(TextWriter writer, object any, Indenter indenter)
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
                    var values = string.Join(" ", target.Cast<object>().Select(one => GetValueToken(one)));
                    writer.WriteLine($"{indenter}{it.nodeName} [ {values} ]");
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

                {
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

        private static ICharStream FromString(string script, string sourceName)
        {
            var stream = CharStreams.fromString(script);
            if (stream is CodePointCharStream charStream)
            {
                charStream.name = sourceName;
            }
            return stream;
        }

        private interface IStatementDeserializer
        {
            void Property(string name, string value);
            IStatementDeserializer Block(string name);
            object GetContainer();
        }

        private class NoopDeser : IStatementDeserializer
        {
            public IStatementDeserializer Block(string name) => this;

            public object GetContainer() => throw new NotSupportedException();

            public void Property(string name, string value) { }
        }

        private class ObjectDeser : IStatementDeserializer
        {
            private readonly DeserHelper _helper;
            private readonly Type _targetType;
            private readonly object _target;

            public ObjectDeser(DeserHelper helper, object target)
            {
                _targetType = target.GetType();
                _target = target;
                _helper = helper;
            }

            public IStatementDeserializer Block(string name)
            {
                var prop = _targetType.GetProperty(name);
                if (prop != null)
                {
                    var propType = prop.PropertyType;

                    if (IsValueTokenType(propType))
                    {
                        throw new Exception("No scalar property for block");
                    }
                    else if (propType is ICollection)
                    {
                        var sub = new CollectionDeser(_helper);
                        prop.SetValue(name, sub.GetContainer());
                        return sub;
                    }
                    else
                    {
                        var sub = new ObjectDeser(_helper, null);
                        prop.SetValue(name, sub.GetContainer());
                        return sub;
                    }
                }

                return new NoopDeser();
            }

            public object GetContainer()
            {
                return _target;
            }

            public void Property(string name, string value)
            {
                var prop = _targetType.GetProperty(name);
                if (prop != null)
                {
                    prop.SetValue(_target, value);
                }
            }
        }

        private class CollectionDeser : IStatementDeserializer
        {
            private readonly DeserHelper _helper;
            private readonly List<object> _items = new List<object>();

            public CollectionDeser(DeserHelper helper)
            {
                _helper = helper;
            }

            public IStatementDeserializer Block(string name)
            {
                var target = _helper.CreateObject(name);
                _items.Add(target);
                return new ObjectDeser(_helper, null);
            }

            public object GetContainer()
            {
                return _items;
            }

            public void Property(string name, string value)
            {
                throw new Exception("Collection cannot declare property");
            }
        }

        private class DeserHelper
        {
            private TreeOptions _options;

            public DeserHelper(TreeOptions options)
            {
                _options = options;
            }

            public object CreateObject(string name)
            {
                if (_options.ObjectTypes.TryGetValue(name, out Type type))
                {
                    return type.InvokeMember(null, BindingFlags.CreateInstance, null, null, new object[0]);
                }
                else
                {
                    throw new Exception($"Object type {name} not registered");
                }
            }
        }

        private static void DeserializeCore(
            StatementContext[] statements,
            IStatementDeserializer statementDeserializer
        )
        {
            foreach (var statement in statements)
            {
                if (statement.property() is PropertyContext propertyContext)
                {
                    statementDeserializer.Property(
                        propertyContext.name.GetText(),
                        propertyContext.value.GetText()
                    );
                }
                else if (statement.block() is BlockContext blockContext)
                {
                    var subDes = statementDeserializer.Block(blockContext.name.GetText());
                    DeserializeCore(
                        blockContext.statement(),
                        subDes
                    );
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        public static T Deserialize<T>(string body, TreeOptions options = null) where T : class
        {
            var stream = CharStreams.fromString(body);
            var lexer = new TreeScriptLexer(stream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new TreeScriptParser(tokens);

            if (parser.script() is ScriptContext scriptContext)
            {
                var deser = GetRootStatementDeser(typeof(T), new DeserHelper(options));
                DeserializeCore(scriptContext.statement(), deser);
                return (T)deser.GetContainer();
            }

            return null;
        }

        private static IStatementDeserializer GetRootStatementDeser(Type type, DeserHelper helper)
        {
            if (IsValueTokenType(type))
            {
                throw new Exception("No scalar type here");
            }

            var isCollection = typeof(ICollection).IsAssignableFrom(type);
            if (isCollection)
            {
                return new CollectionDeser(helper);
            }
            else
            {
                return new ObjectDeser(helper, null);
            }
        }

    }
}
