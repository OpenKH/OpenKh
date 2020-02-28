using OpenKh.Tools.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xe.IO;

namespace OpenKh.Tools.Kh2MapCollisionEditor
{
    public static class ProcessService
    {
        public static IEnumerable<Process> GetPcsx2Processes() =>
            ProcessStream.GetProcesses().Where(x => x.ProcessName == "pcsx2");

        public static ProcessStream OpenPcsx2ProcessStream(Process process, uint baseAddress = 0) =>
            new ProcessStream(process, ToolsConstants.Pcsx2BaseAddress + baseAddress, ToolsConstants.Ps2MemoryLength - baseAddress);

        public static IEnumerable<uint> FilterAddresses(Stream stream, Func<Stream, bool> predicate, int alignment)
        {
            while (stream.Position < stream.Length)
            {
                var currentAddress = stream.Position;
                if (predicate(new SubStream(stream, currentAddress, stream.Length - currentAddress)))
                    yield return (uint)currentAddress;

                stream.Position = currentAddress += alignment;
            }
        }
    }
}
