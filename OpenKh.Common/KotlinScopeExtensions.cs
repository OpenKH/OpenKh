using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Common
{
    // Copy from https://dev.to/amay077/kotlins-scope-functions-in-c-pbn
    public static class KotlinScopeExtensions
    {
        // Kotlin: fun <T, R> T.let(block: (T) -> R): R
        public static R Let<T, R>(this T self, Func<T, R> block)
        {
            return block(self);
        }

        // Kotlin: fun <T> T.also(block: (T) -> Unit): T
        public static T Also<T>(this T self, Action<T> block)
        {
            block(self);
            return self;
        }
    }
}
