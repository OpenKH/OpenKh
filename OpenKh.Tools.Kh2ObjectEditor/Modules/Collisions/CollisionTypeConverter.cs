using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.Collections.Generic;
using static OpenKh.Kh2.ObjectCollision;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Collisions
{
    public class CollisionTypeConverter : BaseEnumStringConverter<TypeEnum>
    {
        public CollisionTypeConverter()
        {
            Options = new Dictionary<TypeEnum, string>
            {
                {TypeEnum.BG, "[0] Background"},
                {TypeEnum.OBJ, "[1] Object"},
                {TypeEnum.HIT, "[2] Hit"},
                {TypeEnum.TARGET, "[3] Target"},
                {TypeEnum.BG_PLAYER, "[4] Background (Player)"},
                {TypeEnum.REACTION, "[5] Reaction"},
                {TypeEnum.ATTACK, "[6] Attack"},
                {TypeEnum.CAMERA, "[7] Camera"},
                {TypeEnum.CAST_ITEM, "[8] Cast Item"},
                {TypeEnum.ITEM, "[9] Item"},
                {TypeEnum.IK, "[10] IK"},
                {TypeEnum.IK_DOWN, "[11] IK Down"},
                {TypeEnum.NECK, "[12] Neck"},
                {TypeEnum.GUARD, "[13] Guard"},
                {TypeEnum.REF_RC, "[14] Ref RC"},
                {TypeEnum.WEAPON_TOP, "[15] Weapon Top"},
                {TypeEnum.STUN, "[16] Stun"},
                {TypeEnum.HEAD, "[17] Head"},
                {TypeEnum.BLIND, "[18] Blind"},
                {TypeEnum.TALKCAMERA, "[19] Talk Camera"},
                {TypeEnum.RTN_NECK, "[20] RTN Neck"},
            };
        }
    }


    //public class CollisionTypeConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        if (value is byte byteValue)
    //        {
    //            if (CollisionTypes.TryGetValue(byteValue, out string stringValue))
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
    //            foreach (var kvp in CollisionTypes)
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
    //    public static Dictionary<byte, string> CollisionTypes = new Dictionary<byte, string>
    //    {
    //        { 0, "Background" },
    //        { 1, "Object" },
    //        { 2, "Hit" },
    //        { 3, "Target" },
    //        { 4, "Background (Player)" },
    //        { 5, "Reaction" },
    //        { 6, "Attack" },
    //        { 7, "Camera" },
    //        { 8, "Cast Item" },
    //        { 9, "Item" },
    //        { 10, "IK" },
    //        { 11, "IK Down" },
    //        { 12, "Neck" },
    //        { 13, "Guard" },
    //        { 14, "Ref RC" },
    //        { 15, "Weapon Top" },
    //        { 16, "Stun" },
    //        { 17, "Head" },
    //        { 18, "Blind" },
    //        { 19, "Talk Camera" },
    //        { 20, "RTN Neck" },
    //    };
    //}
}
