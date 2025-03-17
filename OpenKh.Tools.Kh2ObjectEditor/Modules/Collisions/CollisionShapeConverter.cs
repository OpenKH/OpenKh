using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.Collections.Generic;
using static OpenKh.Kh2.ObjectCollision;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Collisions
{
    public class CollisionShapeConverter : BaseEnumStringConverter<ShapeEnum>
    {
        public CollisionShapeConverter()
        {
            Options = new Dictionary<ShapeEnum, string>
            {
                {ShapeEnum.ELLIPSOID, "[0] Ellipsoid"},
                {ShapeEnum.COLUMN, "[1] Column"},
                {ShapeEnum.CUBE, "[2] Cube"},
                {ShapeEnum.SPHERE, "[3] Sphere"},
            };
        }
    }


    //public class CollisionShapeConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        if (value is byte byteValue)
    //        {
    //            if (CollisionShapes.TryGetValue(byteValue, out string stringValue))
    //            {
    //                return stringValue;
    //            }
    //        }
    //        return value.ToString();
    //    }
    //
    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        if (value is string stringValue && parameter is Dictionary<byte, string> dictionary)
    //        {
    //            foreach (var kvp in CollisionShapes)
    //            {
    //                if (kvp.Value == stringValue)
    //                {
    //                    return kvp.Key;
    //                }
    //            }
    //        }
    //        return value;
    //    }
    //
    //    public static Dictionary<byte, string> CollisionShapes = new Dictionary<byte, string>
    //    {
    //        { 0, "Ellipsoid" },
    //        { 1, "Column" },
    //        { 2, "Cube" },
    //        { 3, "Sphere" },
    //    };
    //}
}
