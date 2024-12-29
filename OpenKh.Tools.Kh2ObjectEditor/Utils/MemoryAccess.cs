using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenKh.Tools.Kh2ObjectEditor.Utils
{
    class MemoryAccess
    {
        //------------------------
        // LIBRARIES
        //------------------------

        [Flags]
        public enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hProcess);

        //------------------------
        // READ
        //------------------------

        // Read bytes from process at a specific address
        public static byte[] readMemory(Process process, long address, long size, bool isGlobalAddress = false)
        {
            var val = new byte[size];
            long globalAddress = isGlobalAddress ? address : (process.MainModule.BaseAddress.ToInt64() + address);
            ReadProcessMemory(process.Handle, new IntPtr(globalAddress), val, (UInt32)val.LongLength, out int bytesRead);
            return val;
        }
        // Shortcuts
        public static byte readByte(Process process, long address, bool isGlobalAddress = false) => readMemory(process, address, 1, isGlobalAddress)[0];
        public static short readShort(Process process, long address, bool isGlobalAddress = false) => BitConverter.ToInt16(readMemory(process, address, 2, isGlobalAddress));
        public static int readInt(Process process, long address, bool isGlobalAddress = false) => BitConverter.ToInt32(readMemory(process, address, 4, isGlobalAddress));
        public static long readLong(Process process, long address, bool isGlobalAddress = false) => BitConverter.ToInt64(readMemory(process, address, 8, isGlobalAddress));
        public static float readFloat(Process process, long address, bool isGlobalAddress = false) => BitConverter.ToSingle(readMemory(process, address, 4, isGlobalAddress));
        public static double readDouble(Process process, long address, bool isGlobalAddress = false) => BitConverter.ToDouble(readMemory(process, address, 8, isGlobalAddress));

        //------------------------
        // WRITE
        //------------------------

        // Write bytes from process at a specific address
        public static void writeMemory(Process process, long address, byte[] input, bool isGlobalAddress = false)
        {
            long globalAddress = isGlobalAddress ? address : (process.MainModule.BaseAddress.ToInt64() + address);
            WriteProcessMemory(process.Handle, new IntPtr(globalAddress), input, (UInt32)input.LongLength, out int bytesWritten);
        }
        // Shortcuts
        public static void writeMemory(Process process, long address, List<byte> input, bool isGlobalAddress = false) => writeMemory(process, address, input.ToArray(), isGlobalAddress);
        public static void writeByte(Process process, long address, byte input, bool isGlobalAddress = false) => writeMemory(process, address, new byte[] { input }, isGlobalAddress);
        public static void writeShort(Process process, long address, short input, bool isGlobalAddress = false) => writeMemory(process, address, BitConverter.GetBytes(input), isGlobalAddress);
        public static void writeInt(Process process, long address, int input, bool isGlobalAddress = false) => writeMemory(process, address, BitConverter.GetBytes(input), isGlobalAddress);
        public static void writeLong(Process process, long address, long input, bool isGlobalAddress = false) => writeMemory(process, address, BitConverter.GetBytes(input), isGlobalAddress);
        public static void writeFloat(Process process, long address, float input, bool isGlobalAddress = false) => writeMemory(process, address, BitConverter.GetBytes(input), isGlobalAddress);
        public static void writeDouble(Process process, long address, double input, bool isGlobalAddress = false) => writeMemory(process, address, BitConverter.GetBytes(input), isGlobalAddress);
    }
}
