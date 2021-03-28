using System;

namespace OpenKh.Common
{
    public static class DisposableExtensions
    {
        public static void Using<T>(this T disposable, Action<T> action)
            where T : IDisposable
        {
            using (disposable)
                action(disposable);
        }

        public static TResult Using<T, TResult>(this T disposable, Func<T, TResult> func)
            where T : IDisposable
        {
            using (disposable)
                return func(disposable);
        }
    }
}
