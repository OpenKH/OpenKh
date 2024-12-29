using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System;
using System.Diagnostics;
using System.Text;

namespace OpenKh.Tools.Kh2ObjectEditor.Services
{
    public class ProcessService
    {
        public static Process KH2Process { get; set; }
        public static string ProcessName = "KINGDOM HEARTS II FINAL MIX";

        public static void locateProcess()
        {
            Process[] processes = Process.GetProcessesByName(ProcessName);
            if (processes.Length == 0)
            {
                Console.WriteLine("Process not found.");
                return;
            }

            KH2Process = processes[0];
        }

        public static long findLocationOfString(string searchString, long readSize = 50000000)
        {
            locateProcess();

            if (KH2Process == null)
                throw new Exception("The process is null");

            byte[] byteArray = MemoryAccess.readMemory(KH2Process, 0, readSize);

            string result = Encoding.ASCII.GetString(byteArray).ToLower();
            long address = result.IndexOf(searchString.ToLower());

            return address;
        }

        public static long getAddressOfFile(string searchString)
        {
            long baseAddress = findLocationOfString(searchString);
            if (baseAddress == 0)
                return 0;

            long fileAddress = MemoryAccess.readLong(KH2Process, baseAddress + 80);
            return fileAddress;
        }
    }
}
