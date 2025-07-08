using System;
using System.Collections.Generic;
using System.Windows.Data;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Motions
{
    public class RangeTriggerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is byte byteValue)
            {
                if (TriggerDictionary.Range.TryGetValue(byteValue, out string stringValue))
                {
                    return stringValue;
                }
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string stringValue)
            {
                foreach (var kvp in TriggerDictionary.Range)
                {
                    if (kvp.Value == stringValue)
                    {
                        return kvp.Key;
                    }
                }
            }
            return value;
        }
    }
}
