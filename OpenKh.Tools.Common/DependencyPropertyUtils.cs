using System;
using System.Windows;

namespace OpenKh.Tools.Common
{
    public static class DependencyPropertyUtils
    {
        public static DependencyProperty GetDependencyProperty<TClass, TValue>(
            string name,
            Action<TClass, TValue> setter,
            Func<TValue, bool> validator = null)
            where TClass : class =>
            GetDependencyProperty(name, default, setter);

        public static DependencyProperty GetDependencyProperty<TClass, TValue>(
            string name,
            TValue defaultValue,
            Action<TClass, TValue> setter,
            Func<TValue, bool> validator = null)
            where TClass : class => DependencyProperty.Register(
                name,
                typeof(TValue),
                typeof(TClass),
                GetProperyMetadata(defaultValue, setter),
                validator == null ?
                    GetValidateDefault<TValue>(defaultValue == null) :
                    GetValidateWithFunc(validator, defaultValue == null));

        public static PropertyMetadata GetProperyMetadata<TClass, TValue>(TValue defalutValue, Action<TClass, TValue> setter)
            where TClass : class => new PropertyMetadata(defalutValue, GetProperyCallback(setter));
        public static PropertyChangedCallback GetProperyCallback<TClass, TValue>(Action<TClass, TValue> setter)
            where TClass : class => new PropertyChangedCallback((d, e) => setter(d as TClass, (TValue)e.NewValue));

        public static ValidateValueCallback GetValidateWithFunc<T>(Func<T, bool> funcValidator, bool canBeNull) =>
            new ValidateValueCallback(x => (x == null && canBeNull) || (x is T value ? funcValidator(value) : false));
        public static ValidateValueCallback GetValidateDefault<T>(bool canBeNull) =>
            new ValidateValueCallback(x => (x == null && canBeNull) || (x is T));
    }
}
