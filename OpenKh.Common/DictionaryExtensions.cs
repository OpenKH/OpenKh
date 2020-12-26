using System.Collections.Generic;

namespace OpenKh.Common
{
    public static class DictionaryExtensions
    {
        public static string GetString(
            this IDictionary<string, string> dic, string key, string defaultValue) =>
            dic.TryGetValue(key, out var value) ? value : defaultValue;

        public static int GetInt(
            this IDictionary<string, string> dic, string key, int defaultValue) =>
            int.TryParse(dic.GetSetting(key, string.Empty), out var value) ? value : defaultValue;

        public static bool GetBool(
            this IDictionary<string, string> dic, string key, bool defaultValue) =>
            bool.TryParse(dic.GetSetting(key, string.Empty), out var value) ? value : defaultValue;

        public static double GetDouble(
            this IDictionary<string, string> dic, string key, double defaultValue) =>
            double.TryParse(dic.GetSetting(key, string.Empty), out var value) ? value : defaultValue;

        public static T GetSetting<T>(
            this IDictionary<string, string> dic, string key, T defaultValue)
        {
            var type = typeof(T);
            if (type == typeof(string))
                return (T)(object)dic.GetString(key, (string)(object)defaultValue);
            if (type == typeof(int))
                return int.TryParse(dic.GetSetting(key, string.Empty), out var value) ?
                    (T)(object)value : defaultValue;
            if (type == typeof(bool))
                return bool.TryParse(dic.GetSetting(key, string.Empty), out var value) ?
                    (T)(object)value : defaultValue;
            if (type == typeof(double))
                return double.TryParse(dic.GetSetting(key, string.Empty), out var value) ?
                    (T)(object)value : defaultValue;

            return defaultValue;
        }
    }
}
