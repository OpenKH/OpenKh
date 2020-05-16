using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
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

		Process process;
		IntPtr hProcess;
		private long position;

		public ProcessStream(Process process)
		{
			OpenProcess(process);
		}


		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => true;

		public override long Length => 32 * 1024 * 1024;

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
			var pos = (IntPtr)(0x20000000 + Position);

			if (offset == 0)
			{
				ReadProcessMemory(hProcess, pos, buffer, count, out read);
			}
			else
			{
				byte[] data = new byte[count];
				ReadProcessMemory(hProcess, pos, buffer, count, out read);
				Array.Copy(data, 0, buffer, offset, read);
			}

			Position += read;

			return read;
		}


		public String ReadString(int length, bool trimEnd, Encoding encoding)
		{
			int stride = (encoding.IsSingleByte ? 1 : 2);
			byte[] buffer = new byte[length * stride];
			Read(buffer, 0, buffer.Length);
			position += buffer.Length;
			if (trimEnd)
			for (int i = 0; i < buffer.Length; i+= stride)
			{
				if (buffer[i] == 0)
					return encoding.GetString(buffer, 0, i);
			}
			return encoding.GetString(buffer);
		}

		public UInt32 ReadUInt32()
		{
			byte[] buffer = new byte[4];
			Read(buffer, 0, buffer.Length);
			position += buffer.Length;
			return global::System.BitConverter.ToUInt32(buffer, 0);
		}

		public Int32 ReadInt32()
		{
			byte[] buffer = new byte[4];
			Read(buffer, 0, buffer.Length);
			position += buffer.Length;
			return global::System.BitConverter.ToInt32(buffer, 0);
		}

		public UInt16 ReadUInt16()
		{
			byte[] buffer = new byte[2];
			Read(buffer, 0, buffer.Length);
			position += buffer.Length;
			return global::System.BitConverter.ToUInt16(buffer, 0);
		}

		public Int16 ReadInt16()
		{
			byte[] buffer = new byte[2];
			Read(buffer, 0, buffer.Length);
			position += buffer.Length;
			return global::System.BitConverter.ToInt16(buffer, 0);
		}

		public SByte ReadSByte()
		{
			byte[] buffer = new byte[1];
			Read(buffer, 0, buffer.Length);
			position += buffer.Length;
			return (SByte)buffer[0];
		}

		public Byte ReadByte()
		{
			byte[] buffer = new byte[1];
			Read(buffer, 0, buffer.Length);
			position += buffer.Length;
			return buffer[0];
		}

		public Single ReadSingle()
		{
			byte[] buffer = new byte[4];
			Read(buffer, 0, buffer.Length);
			position += buffer.Length;
			return global::System.BitConverter.ToSingle(buffer, 0);
		}

		public OpenTK.Matrix4 ReadMatrix4()
		{
			OpenTK.Matrix4 output = OpenTK.Matrix4.Identity;
			byte[] buffer = new byte[64];
			Read(buffer, 0, buffer.Length);
			position += buffer.Length;

			output.M11 = global::System.BitConverter.ToSingle(buffer, 0x00);
			output.M12 = global::System.BitConverter.ToSingle(buffer, 0x04);
			output.M13 = global::System.BitConverter.ToSingle(buffer, 0x08);
			output.M14 = global::System.BitConverter.ToSingle(buffer, 0x0C);

			output.M21 = global::System.BitConverter.ToSingle(buffer, 0x10);
			output.M22 = global::System.BitConverter.ToSingle(buffer, 0x14);
			output.M23 = global::System.BitConverter.ToSingle(buffer, 0x18);
			output.M24 = global::System.BitConverter.ToSingle(buffer, 0x1C);

			output.M31 = global::System.BitConverter.ToSingle(buffer, 0x20);
			output.M32 = global::System.BitConverter.ToSingle(buffer, 0x24);
			output.M33 = global::System.BitConverter.ToSingle(buffer, 0x28);
			output.M34 = global::System.BitConverter.ToSingle(buffer, 0x2C);

			output.M41 = global::System.BitConverter.ToSingle(buffer, 0x30);
			output.M42 = global::System.BitConverter.ToSingle(buffer, 0x34);
			output.M43 = global::System.BitConverter.ToSingle(buffer, 0x38);
			output.M44 = global::System.BitConverter.ToSingle(buffer, 0x3C);

			return output;
		}

		public OpenTK.Matrix4[] ReadMatrices(int count)
		{
			OpenTK.Matrix4[] output = new OpenTK.Matrix4[count];
			byte[] buffer = new byte[64 * count];
			Read(buffer, 0, buffer.Length);
			position += buffer.Length;

			int offset = 0;
			for (int i = 0; i < count; i++)
			{
				output[i] = OpenTK.Matrix4.Identity;
				output[i].M11 = global::System.BitConverter.ToSingle(buffer, offset + 0x00);
				output[i].M12 = global::System.BitConverter.ToSingle(buffer, offset + 0x04);
				output[i].M13 = global::System.BitConverter.ToSingle(buffer, offset + 0x08);
				output[i].M14 = global::System.BitConverter.ToSingle(buffer, offset + 0x0C);

				output[i].M21 = global::System.BitConverter.ToSingle(buffer, offset + 0x10);
				output[i].M22 = global::System.BitConverter.ToSingle(buffer, offset + 0x14);
				output[i].M23 = global::System.BitConverter.ToSingle(buffer, offset + 0x18);
				output[i].M24 = global::System.BitConverter.ToSingle(buffer, offset + 0x1C);

				output[i].M31 = global::System.BitConverter.ToSingle(buffer, offset + 0x20);
				output[i].M32 = global::System.BitConverter.ToSingle(buffer, offset + 0x24);
				output[i].M33 = global::System.BitConverter.ToSingle(buffer, offset + 0x28);
				output[i].M34 = global::System.BitConverter.ToSingle(buffer, offset + 0x2C);

				output[i].M41 = global::System.BitConverter.ToSingle(buffer, offset + 0x30);
				output[i].M42 = global::System.BitConverter.ToSingle(buffer, offset + 0x34);
				output[i].M43 = global::System.BitConverter.ToSingle(buffer, offset + 0x38);
				output[i].M44 = global::System.BitConverter.ToSingle(buffer, offset + 0x3C);
				offset += 0x40;
			}

			return output;
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
			int written = 0;
			var pos = (IntPtr)(0x20000000 + Position);

			if (offset == 0)
			{
				WriteProcessMemory(hProcess, pos, buffer, count, out written);
			}
			else
			{
				var data = new byte[count];
				Array.Copy(buffer, offset, data, 0, count);
				WriteProcessMemory(hProcess, pos, data, count, out written);
			}

			Position += written;
		}

		private void OpenProcess(Process process)
		{
			var permissions = 0x001FFFFF;
			hProcess = OpenProcess(permissions, true, process.Id);
			this.process = process;

			var pos = (IntPtr)(0x20000000 + Position);
			VirtualProtectEx(hProcess, pos, (UIntPtr)Length, 0xFF, out var old);
		}

		public static ProcessStream SearchProcess(string processName)
		{
			while (true)
			{
				foreach (Process process in Process.GetProcesses())
				{
					if (process.ProcessName.Contains(processName))
					{
						return new ProcessStream(process);
					}
				}

				Thread.Sleep(100);
				break;
			}
			return null;
		}

		public static async Task<ProcessStream> SearchProcessAsync(string processName)
		{
			return await Task.Run(() =>
			{
				return SearchProcess(processName);
			});
		}
	}
}
