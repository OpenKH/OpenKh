using OpenKh.Patcher;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Services
{
    internal class PreferenceWrapperService
    {
        internal object GetWrappedObject(
            IDictionary<string, object> dict,
            EasyPrefProcessor.ValueEditor[] editors
        )
        {
            return new WrappedObject(
                dict: dict,
                getProps: () =>
                {
                    return new PropertyDescriptorCollection(
                        editors
                            .Select(
                                editor =>
                                {
                                    return new WrappedProperty(
                                        name: editor.Key,
                                        type: editor.ValueType,
                                        attrs: new Attribute[]
                                        {
                                            new DescriptionAttribute(editor.EasyPref.Description),
                                            new CategoryAttribute(editor.EasyPref.Category),
                                        },
                                        getter: (target) =>
                                        {
                                            var localDict = ((WrappedObject)target).Dict;
                                            localDict.TryGetValue(editor.Key, out object value);
                                            return value;
                                        },
                                        setter: (target, value) =>
                                        {
                                            var localDict = ((WrappedObject)target).Dict;
                                            localDict[editor.Key] = value;
                                        }
                                    );
                                }
                            )
                            .ToArray()
                    );
                }
            );
        }

        internal class WrappedProperty : PropertyDescriptor
        {
            private readonly Type _type;
            private readonly Func<object, object> _getter;
            private readonly Action<object, object> _setter;

            public WrappedProperty(
                string name,
                Type type,
                Attribute[] attrs,
                Func<object, object> getter,
                Action<object, object> setter
            ) : base(name, attrs)
            {
                _type = type;
                _getter = getter;
                _setter = setter;
            }

            public override Type ComponentType => typeof(WrappedObject);

            public override bool IsReadOnly => false;

            public override Type PropertyType => _type;

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override object GetValue(object component)
            {
                return _getter(component);
            }

            public override void ResetValue(object component)
            {
                throw new NotImplementedException();
            }

            public override void SetValue(object component, object value)
            {
                _setter(component, value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }
        }

        internal class WrappedObject : ICustomTypeDescriptor
        {
            internal IDictionary<string, object> Dict { get; }

            private readonly Func<PropertyDescriptorCollection> _getProps;

            public WrappedObject(
                IDictionary<string, object> dict,
                Func<PropertyDescriptorCollection> getProps
            )
            {
                Dict = dict;
                _getProps = getProps;
            }

            public AttributeCollection GetAttributes()
            {
                return new AttributeCollection();
            }

            public string GetClassName()
            {
                return "Wrapper";
            }

            public string GetComponentName()
            {
                return "Wrapper";
            }

            public TypeConverter GetConverter()
            {
                return new TypeConverter();
            }

            public EventDescriptor GetDefaultEvent()
            {
                throw new NotImplementedException();
            }

            public PropertyDescriptor GetDefaultProperty()
            {
                throw new NotImplementedException();
            }

            public object GetEditor(Type editorBaseType)
            {
                throw new NotImplementedException();
            }

            public EventDescriptorCollection GetEvents()
            {
                throw new NotImplementedException();
            }

            public EventDescriptorCollection GetEvents(Attribute[] attributes)
            {
                throw new NotImplementedException();
            }

            public PropertyDescriptorCollection GetProperties()
            {
                return _getProps();
            }

            public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                throw new NotImplementedException();
            }

            public object GetPropertyOwner(PropertyDescriptor pd)
            {
                return this;
            }
        }
    }
}
