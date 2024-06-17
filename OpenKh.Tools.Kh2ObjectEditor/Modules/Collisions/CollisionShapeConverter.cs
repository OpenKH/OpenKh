using System;
using System.Collections.Generic;
using System.Windows.Data;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Collisions
{
    public class CollisionShapeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is byte byteValue)
            {
                if (CollisionShapes.TryGetValue(byteValue, out string stringValue))
                {
                    return stringValue;
                }
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string stringValue && parameter is Dictionary<byte, string> dictionary)
            {
                foreach (var kvp in CollisionShapes)
                {
                    if (kvp.Value == stringValue)
                    {
                        return kvp.Key;
                    }
                }
            }
            return value;
        }

        public static Dictionary<byte, string> CollisionShapes = new Dictionary<byte, string>
        {
            { 0, "Ellipsoid" },
            { 1, "Column" },
            { 2, "Cube" },
            { 3, "Sphere" },
        };
    }
}
