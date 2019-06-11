using System;

namespace OpenKh.Game
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new OpenKhGame())
                game.Run();
        }
    }
}
