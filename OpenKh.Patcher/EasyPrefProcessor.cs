using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Patcher
{
    public class EasyPrefProcessor
    {
        public ValueEditor[] ValueEditors { get; }

        public record ValueEditor(
            string Key,
            Type ValueType,
            Func<object, string> ObjectToString,
            Func<string, object> StringToObject,

            EasyPref EasyPref
        );

        public static readonly ValueEditor Invalid = new ValueEditor(
            "Invalid",
            typeof(DBNull),
            _ => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
            new EasyPref()
        );

        public EasyPrefProcessor(EasyPref[] easyPrefs)
        {
            ValueEditors = (easyPrefs ?? new EasyPref[0])
                .Select(
                    pref =>
                    {
                        Type valueType;
                        Func<object, string> objectToString = obj => obj?.ToString();
                        Func<string, object> stringToObject;
                        switch (pref.ValueType)
                        {
                            default:
                                return Invalid;

                            case "int":
                                valueType = typeof(int);
                                stringToObject = str => Convert.ToInt32(str);
                                break;

                            case "bool":
                                valueType = typeof(bool);
                                stringToObject = str => Convert.ToBoolean(str);
                                break;

                            case "text":
                            case "string":
                                valueType = typeof(string);
                                stringToObject = str => str;
                                break;
                        }

                        return new ValueEditor(
                            Key: pref.Key,
                            ValueType: valueType,
                            ObjectToString: objectToString,
                            StringToObject: stringToObject,
                            EasyPref: pref
                        );
                    }
                )
                .Where(it => !ReferenceEquals(it, Invalid))
                .ToArray();
        }

        public IDictionary<string, object> GetPrefDictionary(
            Func<string, object> getter = null
        )
        {
            var dict = new Dictionary<string, object>();
            foreach (var editor in ValueEditors)
            {
                var preferredValue = getter?.Invoke(editor.Key);
                dict[editor.Key] = (preferredValue != null)
                    ? Convert.ChangeType(preferredValue, editor.ValueType)
                    : editor.StringToObject(editor.EasyPref.DefaultValue);
            }

            return dict;
        }

        public IDictionary<string, string> ConvertBack(IDictionary<string, object> easyPrefs)
        {
            var dict = new Dictionary<string, string>();

            foreach (var editor in ValueEditors)
            {
                if (easyPrefs.TryGetValue(editor.Key, out object value))
                {
                    dict[editor.Key] = editor.ObjectToString(value);
                }
            }

            return dict;
        }
    }
}
