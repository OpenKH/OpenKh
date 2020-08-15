using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Tools.Common
{
    public sealed class LinkToPcsx2 : IDisposable
    {
        public ProcessRef[] Pcsx2Refs { get; }

        public sealed class ProcessRef : IDisposable
        {
            public Process Process { get; }

            public override string ToString() => $"PID={Process.Id}, Name={Process.ProcessName}";

            public ProcessRef(Process p)
            {
                Process = p;
            }

            public Pcsx2ProcessStream OpenStream() => new Pcsx2ProcessStream(Process);

            public void Dispose()
            {
                Process.Dispose();
            }
        }

        public sealed class Pcsx2ProcessStream : IDisposable
        {
            public ProcessStream ProcessStream { get; }
            public BufferedStream BufferedStream { get; }

            public Pcsx2ProcessStream(Process process)
            {
                ProcessStream = new ProcessStream(process, ToolConstants.Pcsx2BaseAddress, ToolConstants.Ps2MemoryLength);
                BufferedStream = new BufferedStream(ProcessStream, 0x10000);
            }

            public LoadedEntry[] GetKH2FMLoadedEntries()
            {
                // Check if this is pcsx2 KH2fm instance, by reading part of `SLPM_666.75` (1130h-113fh)
                BufferedStream.Position = 0x100130;
                var part = Encoding.GetEncoding("latin1").GetString(BufferedStream.ReadBytes(16));
                if (part != "\xFE\x01\x02\x3C\x00\x02\x03\x3C\x00\x00\x42\x24\x00\x00\x63\x24")
                {
                    throw new KH2fmNotFoundException();
                }

                BufferedStream.Position = 0x4F6480;
                return Enumerable.Range(0, 200)
                    .Select(
                        _ =>
                        {
                            var it = BinaryMapping.ReadObject<LoadedEntry>(BufferedStream);
                            it.FileName = it.FileName.Split('\0').First();
                            return it;
                        }
                    )
                    .Where(it => it.Addr1 != 0)
                    .ToArray();
            }

            public void Dispose()
            {
                BufferedStream.Dispose();
                ProcessStream.Dispose();
            }
        }

        public class KH2fmNotFoundException : Exception
        {
            public KH2fmNotFoundException() : base("KH2fm is not found in this PCSX2.")
            {

            }
        }

        public class LoadedEntry
        {
            [Data] public uint Unk0 { get; set; }
            [Data(Count = 24)] public string FileName { get; set; }
            [Data(Count = 16)] public byte[] Unk1 { get; set; }
            [Data] public int Len { get; set; }
            [Data] public int Addr1 { get; set; }
            [Data] public int Addr2 { get; set; }
            [Data(Count = 24)] public byte[] Unk2 { get; set; }

            public override string ToString() => $"{Addr1:X8}-{Addr1 + Len - 1:X8} {FileName}";
        }

        public LinkToPcsx2()
        {
            Pcsx2Refs = Process.GetProcessesByName("pcsx2")
                .Select(p => new ProcessRef(p))
                .ToArray();
        }

        public void Dispose()
        {
            foreach (var p in Pcsx2Refs)
            {
                p.Dispose();
            }
        }
    }
}
