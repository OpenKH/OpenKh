using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace OpenKh.Tools.Kh2ObjectEditor.Utils
{
    public class BaseEnumStringConverter<T> : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is T entryType && Options.TryGetValue(entryType, out string displayString))
            {
                return displayString;
            }

            throw new Exception("No match found for the value: " + value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var kvp in Options)
            {
                if (kvp.Value.Equals(value as string, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Key;
                }
            }

            throw new Exception("No match found for the value: " + value);
        }

        public Dictionary<T, string> Options = new Dictionary<T, string>();
    }
}
