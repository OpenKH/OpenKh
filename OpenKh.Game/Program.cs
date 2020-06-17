using OpenKh.Game.Debugging;
using System;
using System.Reflection;

namespace OpenKh.Game
{
    public static class Program
    {
        public static readonly string ProductVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [STAThread]
        static void Main(string[] args)
        {
            Log.Info("Boot");
            Log.Info($"Version {ProductVersion}");

#if DEBUG
            using (var game = new OpenKhGame(args))
                game.Run();
#else
            try
            {
                using (var game = new OpenKhGame(args))
                        game.Run();
            }
            catch (Exception ex)
            {
                Log.Err("A fatal error has occurred. Please attach this log to https://github.com/xeeynamo/openkh/issues");
                Catch(ex);
                Log.Close();

                throw ex;
            }
#endif

            Log.Info("End");
            Log.Close();
        }

        private static void Catch(Exception ex)
        {
            Log.Err($"{ex.GetType().Name}: {ex.Message}:\n{ex.StackTrace}");
            if (ex.InnerException != null)
                Catch(ex.InnerException);
        }
    }
}
