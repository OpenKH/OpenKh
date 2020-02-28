using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace OpenKh.Tools.Common
{
	public class ProcessStream : Stream
	{
		[DllImport("kernel32.dll")]
		private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

		[DllImport("kernel32.dll")]
		private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int BytesRead);

		[DllImport("kernel32.dll")]
		static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);


		[DllImport("kernel32.dll")]
		static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

		private Process _process;
		private IntPtr _hProcess;
		private long position;

		public ProcessStream(Process process, uint baseAddress, uint length)
		{
			OpenProcess(process);
			BaseAddress = baseAddress;
			Length = length;
		}


		public uint BaseAddress { get; }
		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => true;

		public override long Length { get; }

		public override long Position
		{
			get => position;
			set => position = value;
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int read;
			var pos = (IntPtr)(BaseAddress + Position);

			if (offset == 0)
			{
				ReadProcessMemory(_hProcess, pos, buffer, count, out read);
			}
			else
			{
				byte[] data = new byte[count];
				ReadProcessMemory(_hProcess, pos, buffer, count, out read);
				Array.Copy(data, 0, buffer, offset, read);
			}

			Position += read;

			return read;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					return Position = offset;
				case SeekOrigin.Current:
					return Position += offset;
				case SeekOrigin.End:
					return Position = Length + offset;
				default:
					return Position;
			}
		}

		public override void SetLength(long value)
		{
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			var pos = (IntPtr)(BaseAddress + Position);

			int written;
			if (offset == 0)
			{
				WriteProcessMemory(_hProcess, pos, buffer, count, out written);
			}
			else
			{
				var data = new byte[count];
				Array.Copy(buffer, offset, data, 0, count);
				WriteProcessMemory(_hProcess, pos, data, count, out written);
			}

			Position += written;
		}

		private void OpenProcess(Process process)
		{
			const int permissions = 0x001FFFFF;
			_hProcess = OpenProcess(permissions, true, process.Id);
			_process = process;

			var pos = (IntPtr)(BaseAddress + Position);
			VirtualProtectEx(_hProcess, pos, (UIntPtr)Length, 0xFF, out var old);
		}

		public static IEnumerable<Process> GetProcesses() =>
			Process.GetProcesses();

		public static Process TryGetProcess(Func<Process, bool> predicate, int timeout = 10000, int sleep = 100)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			do
			{
				var process = GetProcesses().FirstOrDefault(predicate);
				if (process != null)
					return process;

				Thread.Sleep(sleep);
			} while (stopwatch.ElapsedMilliseconds < timeout);

			return null;
		}

		public static Task<Process> TryGetProcessAsync(Func<Process, bool> predicate, int timeout = 10000, int sleep = 100) =>
			Task.Run(() => TryGetProcess(predicate, timeout, sleep));
	}
}
