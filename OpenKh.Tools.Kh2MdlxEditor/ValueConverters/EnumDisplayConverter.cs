using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace OpenKh.Tools.Kh2MdlxEditor.ValueConverters
{
    public class EnumDisplayConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string typeName)
            {
                if (Type.GetType(typeName, false) is Type enumType)
                {
                    return enumType.GetEnumName(value);
                }
            }

            return System.Convert.ChangeType(value, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
