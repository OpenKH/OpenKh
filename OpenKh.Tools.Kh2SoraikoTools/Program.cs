using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2SoraikoTools
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new Game())
                game.Run();
        }

    }
}
