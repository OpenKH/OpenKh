/*
    Kingdom Save Editor
    Copyright (C) 2020 Luciano Ciccariello

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using OpenKh.Tools.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Services
{
    public static class Pcsx2MemoryService
    {
        private const uint Pcsx2EmulationBaseAddress = 0x20000000;
        private const uint Pcsx2EmulationMemoryLength = 0x2000000;
        private const uint BootFileMaximumStringLength = 0x20;
        private static readonly uint[] PlayStation2BootFileOffsets = new uint[]
        {
            0x12610, // Japan v1.00
            0x15390, // American 1.60
            0x15510, // Europe v1.60
            0x155D0, // Europe v2.00
            0x15B90, // Every other BIOS from v2.20 or above
        };

        private class GameEntry
        {
            public string BootFile { get; }
            public long Offset { get; }
            public int Length { get; }

            public GameEntry(string titleId, long offset, int length)
            {
                BootFile = titleId;
                Offset = offset;
                Length = length;
            }
        }

        private static GameEntry[] Games => new GameEntry[]
        {
            new GameEntry("SLPM_610.25;1", 0x203F1210, 0x16c00), // Kingdom Hearts I Premium Showcase
            new GameEntry("SLPS_251.05;1", 0x203F2080, 0x16c00), // Kingdom Hearts I (JP)
            new GameEntry("SLUS_203.70;1", 0x203F1C90, 0x16c00), // Kingdom Hearts I (US)
            new GameEntry("SCUS_203.70;1", 0x203F1C90, 0x16c00), // Kingdom Hearts I (US)
            new GameEntry("SLES_509.67;1", 0x203F22C0, 0x16c00), // Kingdom Hearts I (UK)
            new GameEntry("SLES_509.68;1", 0x203F1AB0, 0x16c00), // Kingdom Hearts I (FR)
            new GameEntry("SLES_509.69;1", 0x203F3CF0, 0x16c00), // Kingdom Hearts I (DE)
            new GameEntry("SLES_509.70;1", 0x203F3900, 0x16c00), // Kingdom Hearts I (IT)
            new GameEntry("SLES_509.71;1", 0x203F4620, 0x16c00), // Kingdom Hearts I (ES)
            new GameEntry("SCES_509.67;1", 0x203F22C0, 0x16c00), // Kingdom Hearts I (UK)
            new GameEntry("SCES_509.68;1", 0x203F1AB0, 0x16c00), // Kingdom Hearts I (FR)
            new GameEntry("SCES_509.69;1", 0x203F3CF0, 0x16c00), // Kingdom Hearts I (DE)
            new GameEntry("SCES_509.70;1", 0x203F3900, 0x16c00), // Kingdom Hearts I (IT)
            new GameEntry("SCES_509.71;1", 0x203F4620, 0x16c00), // Kingdom Hearts I (ES)
            new GameEntry("SLPS_251.97;1", 0x203F8380, 0x16c00), // Kingdom Hearts I: Final Mix
            new GameEntry("SLPS_251.98;1", 0x203F8380, 0x16c00), // Kingdom Hearts I: Final Mix

            new GameEntry("SLPM_662.33;1", 0x2033DCE0, 0xb830), // Kingdom Hearts II (JP)
            new GameEntry("SLUS_210.05;1", 0x2033E860, 0xb4e0), // Kingdom Hearts II (US)
            new GameEntry("SCUS_210.05;1", 0x2033E860, 0xb4e0), // Kingdom Hearts II (US)
            new GameEntry("SLES_541.44;1", 0x2033ED60, 0xb4e0), // Kingdom Hearts II (UK)
            new GameEntry("SLES_542.32;1", 0x2033ED60, 0xb4e0), // Kingdom Hearts II (FR)
            new GameEntry("SLES_542.33;1", 0x2033ED60, 0xb4e0), // Kingdom Hearts II (DE)
            new GameEntry("SLES_542.34;1", 0x2033ED60, 0xb4e0), // Kingdom Hearts II (IT)
            new GameEntry("SCES_542.35;1", 0x2033ED60, 0xb4e0), // Kingdom Hearts II (ES)
            new GameEntry("SCES_541.44;1", 0x2033ED60, 0xb4e0), // Kingdom Hearts II (UK)
            new GameEntry("SCES_542.32;1", 0x2033ED60, 0xb4e0), // Kingdom Hearts II (FR)
            new GameEntry("SCES_542.33;1", 0x2033ED60, 0xb4e0), // Kingdom Hearts II (DE)
            new GameEntry("SCES_542.34;1", 0x2033ED60, 0xb4e0), // Kingdom Hearts II (IT)
            new GameEntry("SCES_542.35;1", 0x2033ED60, 0xb4e0), // Kingdom Hearts II (ES)
            new GameEntry("SLPM_666.75;1", 0x2032BB30, 0x10fc0), // Kingdom Hearts II: Final Mix
        };

        public static async Task<string> GetPcsx2ApplicationName(Process process, CancellationToken cancellationToken)
        {
            int byteReadCount;
            var data = new byte[BootFileMaximumStringLength];

            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var offsets in PlayStation2BootFileOffsets)
                {
                    using (var searchStream = new ProcessStream(process, Pcsx2EmulationBaseAddress + offsets, 0x20))
                    {
                        byteReadCount = searchStream.Read(data, 0, data.Length);
                    }

                    var bootFile = Encoding.ASCII.GetString(data.TakeWhile(b => !b.Equals(0)).ToArray());

                    // Here the situation becomes weird. We can have 5 possible different scenario:
                    // 1. The emulator is not booted. so the selected portion of memory will be undefined.
                    //    The task can wait for the user to load the game.
                    //    We can impose a quite long sleep, since it takes time from user's interaction.
                    if (byteReadCount == 0)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    // 2. The emulator is booted to the bios, which will return "SYS"
                    //    or "LOGO". The task can wait for the BIOS to boot the game.
                    //    The boot can happen at any time... or not happen at all.
                    if (bootFile == "SYS" || bootFile == "LOGO")
                    {
                        await Task.Delay(1);
                        continue;
                    }

                    // 3. The game is booting, so the length of bootFile will be 0.
                    //    The task can wait for the game to be booted.
                    //    This operation is usually quite fast.
                    if (bootFile.Length == 0)
                    {
                        await Task.Delay(1);
                        continue;
                    }

                    // 4. A random string can be found for a fracion of second.
                    //    Since a game always starts with "SL" or "SC", prevents
                    //    to consider that random string as something useful.
                    if (bootFile.StartsWith("SL") || bootFile.StartsWith("SC"))
                        return bootFile;

                    await Task.Delay(1);
                }
            }

            return null;
        }

        public static async Task<ProcessStream> CreateStreamFromPcsx2Process(Process process, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var bootFile = await GetPcsx2ApplicationName(process, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return null;

                var game = Games.FirstOrDefault(x => x.BootFile == bootFile);
                if (game == null)
                    return null;

                return new ProcessStream(process, (uint)game.Offset, (uint)game.Length);
            }

            return null;
        }
    }
}
