using Antlr4.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static TreeScriptParser;

namespace OpenKh.DeeperTree
{
    public class TreeReader
    {
        private readonly DeserHelper _deserHelper;

        internal TreeReader(TreeReaderBuilder builder)
        {
            _deserHelper = new DeserHelper(builder);
        }

        private interface IStatementDeserializer
        {
            void Property(string name, string value);
            void Property2(string name, string[] values);
            (IStatementDeserializer, Action) Block(string name);
            object GetContainer();
        }

        private class NoopDeserializer : IStatementDeserializer
        {
            public (IStatementDeserializer, Action) Block(string name) => (this, () => { });

            public object GetContainer() => throw new NotSupportedException();

            public void Property(string name, string value) { }

            public void Property2(string name, string[] values) { }
        }

        private class ObjectParentDeserializer : IStatementDeserializer
        {
            private bool _set = false;
            private readonly DeserHelper _helper;
            private readonly Action<object> _setValue;

            public ObjectParentDeserializer(DeserHelper helper, Action<object> setValue)
            {
                _helper = helper;
                _setValue = setValue;
            }

            public (IStatementDeserializer, Action) Block(string name)
            {
                if (_set)
                {
                    throw new Exception("Object block must contain only single block");
                }
                else
                {
                    _set = true;
                }

                var target = _helper.CreateObject(name);
                _setValue(target);
                return (
                    new ObjectDeserializer(_helper, target, target.GetType()), () => { }
                );
            }

            public object GetContainer()
            {
                throw new NotImplementedException();
            }

            public void Property(string name, string value)
            {
                throw new Exception("Object block must contain only single block or emtpy");
            }

            public void Property2(string name, string[] values)
            {
                throw new Exception("Object block must contain only single block or emtpy");
            }
        }

        private class ObjectDeserializer : IStatementDeserializer
        {
            private readonly DeserHelper _helper;
            private readonly Type _targetType;
            private readonly object _target;

            public ObjectDeserializer(DeserHelper helper, object target, Type targetType)
            {
                _targetType = targetType;
                _target = target;
                _helper = helper;
            }

            public (IStatementDeserializer, Action) Block(string name)
            {
                var prop = _targetType.GetProperty(name);
                if (prop != null)
                {
                    var propType = prop.PropertyType;

                    if (TreeHelper.IsPrimitiveValue(propType))
                    {
                        throw new Exception("No scalar property for block");
                    }
                    else if (typeof(ICollection).IsAssignableFrom(propType))
                    {
                        var sub = new CollectionDeserializer(_helper, propType);
                        return (
                            sub, () => prop.SetValue(_target, sub.GetContainer())
                        );
                    }
                    else
                    {
                        return (
                            new ObjectParentDeserializer(_helper, value => prop.SetValue(_target, value)), () => { }
                        );
                    }
                }

                return (
                    new NoopDeserializer(), () => { }
                );
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
                    prop.SetValue(_target, ConvertHelper.ChangeType(value, prop.PropertyType));
                }
            }

            public void Property2(string name, string[] values)
            {
                var prop = _targetType.GetProperty(name);
                if (prop != null)
                {
                    var elementType = prop.PropertyType.GetElementType();

                    var objects = values
                        .Select(value => ConvertHelper.ChangeType(value, elementType))
                        .ToList();

                    prop.SetValue(_target, _helper.CreateCollection(prop.PropertyType, objects));
                }
            }
        }

        private static class ConvertHelper
        {
            internal static object ChangeType(string value, Type type)
            {
                if (type.IsEnum && value is string text)
                {
                    return Enum.Parse(type, text);
                }
                return Convert.ChangeType(value, type);
            }
        }

        private class CollectionDeserializer : IStatementDeserializer
        {
            private readonly DeserHelper _helper;
            private readonly Type _type;
            private readonly List<object> _items = new List<object>();

            public CollectionDeserializer(DeserHelper helper, Type type)
            {
                _helper = helper;
                _type = type;
            }

            public (IStatementDeserializer, Action) Block(string name)
            {
                var target = _helper.CreateObject(name);
                _items.Add(target);
                return (
                    new ObjectDeserializer(_helper, target, target.GetType()),
                    () => { }
                );
            }

            public object GetContainer()
            {
                return _helper.CreateCollection(_type, _items);
            }

            public void Property(string name, string value)
            {
                throw new Exception("Collection cannot declare property");
            }

            public void Property2(string name, string[] values)
            {
                throw new Exception("Collection cannot declare property");
            }
        }

        private class DeserHelper
        {
            private readonly SortedDictionary<string, Type> _objTypes;

            public DeserHelper(TreeReaderBuilder options)
            {
                _objTypes = new SortedDictionary<string, Type>(options.ObjectTypes);
            }

            public object CreateObject(Type type) => Activator.CreateInstance(type);

            public object CreateObject(string name)
            {
                if (_objTypes.TryGetValue(name, out Type type))
                {
                    return CreateObject(type);
                }
                else
                {
                    throw new Exception($"Call `treeOptions.AddType(\"{name}\", typeof({name}))`");
                }
            }

            public object CreateCollection(Type type, List<object> items)
            {
                if (type.IsArray)
                {
                    var array = Array.CreateInstance(type.GetElementType(), items.Count);
                    for (int idx = 0; idx < items.Count; idx++)
                    {
                        array.SetValue(items[idx], idx);
                    }
                    return array;
                }
                else
                {
                    var elementType = type.GetInterfaces()
                        .Single(it => it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IList<>))
                        .GetGenericArguments()
                        .First();

                    var list = Activator.CreateInstance(
                        typeof(List<>).MakeGenericType(elementType)
                    );
                    foreach (var item in items)
                    {
                        ((IList)list).Add(item);
                    }

                    return list;
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
                    var bare = propertyContext.value.Bare();
                    var quoted = propertyContext.value.Quoted();

                    statementDeserializer.Property(
                        propertyContext.name.GetText(),
                        (bare != null)
                            ? bare.GetText() 
                            : Unquote(quoted.GetText())
                    );
                }
                else if (statement.array() is ArrayContext arrayContext)
                {
                    statementDeserializer.Property2(
                        arrayContext.name.GetText(),
                        arrayContext.element()
                            .Select(it => it.GetText())
                            .ToArray()
                    );
                }
                else if (statement.block() is BlockContext blockContext)
                {
                    var (subDes, onExit) = statementDeserializer.Block(blockContext.name.GetText());
                    DeserializeCore(
                        blockContext.statement(),
                        subDes
                    );
                    onExit();
                }
                else
                {
                    // empty line
                }
            }
        }

        private static string Unquote(string body)
        {
            var stream = CharStreams.fromString(body);
            var lexer = new Quoted(stream);
            var text = "";
            while (true)
            {
                var token = lexer.NextToken();
                if (token.Type == -1)
                {
                    //<EOF>
                    break;
                }
                switch (token.Type)
                {
                    case Quoted.NL:
                        text += token.Text.Substring(1);
                        break;
                    case Quoted.Cr:
                        text += "\r";
                        break;
                    case Quoted.Lf:
                        text += "\n";
                        break;
                    case Quoted.Tab:
                        text += "\t";
                        break;
                    case Quoted.EscapedChar:
                        text += token.Text.Substring(1);
                        break;
                    case Quoted.AnyChar:
                        text += token.Text;
                        break;
                }
            }
            return text;
        }

        public T Deserialize<T>(string body) where T : class
        {
            var stream = CharStreams.fromString(body);
            var lexer = new TreeScriptLexer(stream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new TreeScriptParser(tokens);

            if (parser.script() is ScriptContext scriptContext)
            {
                var deser = GetRootStatementDeser(typeof(T), _deserHelper);
                DeserializeCore(scriptContext.statement(), deser);
                return (T)deser.GetContainer();
            }

            return null;
        }

        private static IStatementDeserializer GetRootStatementDeser(Type type, DeserHelper helper)
        {
            if (TreeHelper.IsPrimitiveValue(type))
            {
                throw new Exception("No scalar type here");
            }

            var isCollection = typeof(ICollection).IsAssignableFrom(type);
            if (isCollection)
            {
                return new CollectionDeserializer(helper, type);
            }
            else
            {
                var obj = helper.CreateObject(type);
                return new ObjectDeserializer(helper, obj, type);
            }
        }
    }
}
