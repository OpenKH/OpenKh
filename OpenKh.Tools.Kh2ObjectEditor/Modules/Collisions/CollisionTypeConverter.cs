using System;
using System.Collections.Generic;
using System.Windows.Data;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Collisions
{
    public class CollisionTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is byte byteValue)
            {
                if (CollisionTypes.TryGetValue(byteValue, out string stringValue))
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
                foreach (var kvp in CollisionTypes)
                {
                    if (kvp.Value == stringValue)
                    {
                        return kvp.Key;
                    }
                }
            }
            return value;
        }

        public static Dictionary<byte, string> CollisionTypes = new Dictionary<byte, string>
        {
            { 0, "Background" },
            { 1, "Object" },
            { 2, "Hit" },
            { 3, "Target" },
            { 4, "Background (Player)" },
            { 5, "Reaction" },
            { 6, "Attack" },
            { 7, "Camera" },
            { 8, "Cast Item" },
            { 9, "Item" },
            { 10, "IK" },
            { 11, "IK Down" },
            { 12, "Neck" },
            { 13, "Guard" },
            { 14, "Ref RC" },
            { 15, "Weapon Top" },
            { 16, "Stun" },
            { 17, "Head" },
            { 18, "Blind" },
            { 19, "Talk Camera" },
            { 20, "RTN Neck" },
        };
    }
}
