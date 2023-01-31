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

        public EasyPrefProcessor(EasyPref[] easyPrefs)
        {
            ValueEditors = easyPrefs
                .Select(
                    pref =>
                    {
                        Type valueType;
                        Func<object, string> objectToString = obj => obj?.ToString();
                        Func<string, object> stringToObject;
                        switch (pref.ValueType)
                        {
                            default:
                                valueType = typeof(string);
                                stringToObject = str => str;
                                break;

                            case "int":
                                valueType = typeof(int);
                                stringToObject = str => Convert.ToInt32(str);
                                break;

                            case "bool":
                                valueType = typeof(bool);
                                stringToObject = str => Convert.ToBoolean(str);
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
    }
}
