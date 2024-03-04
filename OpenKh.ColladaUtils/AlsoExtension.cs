using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.ColladaUtils
{
    /// <seealso cref="https://kotlinlang.org/docs/scope-functions.html"/>
    internal static class AlsoExtension
    {
        /// <seealso cref="https://kotlinlang.org/api/latest/jvm/stdlib/kotlin/also.html"/>
        public static T Also<T>(this T obj, Action<T> action)
        {
            action(obj);
            return obj;
        }
    }
}
