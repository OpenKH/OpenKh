using OpenKh.Game.Debugging;
using System;

namespace OpenKh.Game
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            Log.Info("Boot");

            try
            {
                using (var game = new OpenKhGame())
                    game.Run();
            }
            catch (Exception ex)
            {
                Log.Err("A fatal error has occurred. Please attach this log to https://github.com/xeeynamo/openkh/issues");
                Catch(ex);
                Log.Flush();

                throw ex;
            }

            Log.Info("End");
            Log.Flush();
        }

        private static void Catch(Exception ex)
        {
            Log.Err($"{ex.GetType().Name}: {ex.Message}:\n{ex.StackTrace}");
            if (ex.InnerException != null)
                Catch(ex.InnerException);
        }
    }
}
