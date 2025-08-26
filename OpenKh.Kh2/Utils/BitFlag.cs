using System;

namespace OpenKh.Kh2.Utils
{
    public class BitFlag
    {
        // Ensure the type is an enum and supports flags
        private static void ValidateEnumType<T>() where T : Enum
        {
            if (!typeof(T).IsDefined(typeof(FlagsAttribute), false))
            {
                throw new ArgumentException($"{typeof(T).Name} must have the [Flags] attribute.");
            }
        }

        // Check if a flag is set
        public static bool IsFlagSet<T>(T value, T flag) where T : Enum
        {
            ValidateEnumType<T>();
            var intValue = Convert.ToInt32(value);
            var intFlag = Convert.ToInt32(flag);
            return (intValue & intFlag) == intFlag;
        }

        // Set a flag
        public static T SetFlag<T>(T value, T flag) where T : Enum
        {
            ValidateEnumType<T>();
            var intValue = Convert.ToInt32(value);
            var intFlag = Convert.ToInt32(flag);
            return (T)Enum.ToObject(typeof(T), intValue | intFlag);
        }

        // Set a flag to given value
        public static T SetFlag<T>(T value, T flag, bool setValue) where T : Enum
        {
            if (setValue)
            {
                return SetFlag(value, flag);
            }
            else
            {
                return ClearFlag(value, flag);
            }
        }

        // Clear a flag
        public static T ClearFlag<T>(T value, T flag) where T : Enum
        {
            ValidateEnumType<T>();
            var intValue = Convert.ToInt32(value);
            var intFlag = Convert.ToInt32(flag);
            return (T)Enum.ToObject(typeof(T), intValue & ~intFlag);
        }

        // Toggle a flag
        public static T ToggleFlag<T>(T value, T flag) where T : Enum
        {
            ValidateEnumType<T>();
            var intValue = Convert.ToInt32(value);
            var intFlag = Convert.ToInt32(flag);
            return (T)Enum.ToObject(typeof(T), intValue ^ intFlag);
        }
    }
}
