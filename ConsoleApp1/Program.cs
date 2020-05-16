using OpenKh.Common;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
	class Program
	{
		static void Main(string[] args)
		{
			string OpenKhSolutionDirectory = 
				Directory.GetParent( /* ConsoleApp1 */
					Directory.GetParent( /* bin */
						Directory.GetParent( /* Debug */
							Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName).FullName;

			if (!Directory.Exists(OpenKhSolutionDirectory + @"\OpenKh.Tests"))
				return;
			Directory.SetCurrentDirectory(OpenKhSolutionDirectory + @"\OpenKh.Tests");


			Stream stream = ProcessStream.SearchProcess("pcsx2"); /* For RAM-related tests. */

			//stream.Position = 0x00964c10;
			//var dpd = new OpenKh.Kh2.Dpd(stream);
			//var dpd = new OpenKh.Kh2.Dpd(File.OpenRead(@"D:\Hacking\KH2\reverse\SAMPLES\tt_0.dpd"));
			//var dpd = new OpenKh.Kh2.Dpd(File.OpenRead(@"D:\Hacking\KH2\reverse\SAMPLES\texcommon.dpd"));


			RunSoraikoTest(ref stream);
		}



		public static void RunSoraikoTest(ref Stream stream)
		{
			/* 16/05/2020
			 * Custom Mdlx Reader by Soraiko, for two major purposes:
			 * 
				-Allows to read and save a Mdlx or Map from a file OR from the RAM of PCSX2 directly
				-Allows to export a DAE model conserving the 3D data order and indexation.
			 *
			*/

			string[] folder_names = new string[] {
				"PLAYER_SLOT",
				"PARTNER1_SLOT",
				"PARTNER2_SLOT",
				"MAP"
			};
			long[] offsets = new long[] {
			SrkAlternatives.Mdlx.RAM_PARTY_POINTER_PLAYER,
			SrkAlternatives.Mdlx.RAM_PARTY_POINTER_PARTNER1,
			SrkAlternatives.Mdlx.RAM_PARTY_POINTER_PARTNER2,
			SrkAlternatives.Mdlx.RAM_MAP_POINTER
			};

			for (int i=0;i<offsets.Length;i++)
			{
				string model_filename = "";
				try
				{
					if (stream == null)
						throw new Exception("PCSX2 process not open.");
					if (i < 3)
					{
						stream.Position = offsets[i];
						stream.Seek(stream.ReadInt32(), SeekOrigin.Begin); stream.Position += 8;
						stream.Seek(stream.ReadInt32(), SeekOrigin.Begin); stream.Position += 8;

						model_filename = stream.ReadString(32, Encoding.ASCII).TrimEnd('\x0');
						stream.Position = offsets[i];
						stream.Seek(stream.ReadInt32(), SeekOrigin.Begin); stream.Position += 0x7AC;
						stream.Seek(stream.ReadInt32(), SeekOrigin.Begin);
					}
					else
					{
						stream.Position = offsets[i] + 0x6F;
						model_filename = stream.ReadString(4, Encoding.ASCII).TrimEnd('\x0');
						stream.Position = offsets[i] + 0x88;
						stream.Seek(stream.ReadInt32(), SeekOrigin.Begin);
					}

					if (!Directory.Exists(@"kh2\res\ram_exported"))
					{
						Directory.CreateDirectory(@"kh2\res\ram_exported");
						Process.Start(@"kh2\res\ram_exported");
					}
					if (!Directory.Exists(@"kh2\res\ram_exported\"+ folder_names[i]))
						Directory.CreateDirectory(@"kh2\res\ram_exported\"+ folder_names[i]);

					SrkAlternatives.Mdlx mdlx = new SrkAlternatives.Mdlx(stream);

					mdlx.Save(@"kh2\res\ram_exported\" + folder_names[i] + @"\" + model_filename + (i < 3 ? ".mdlx" : ".map"));
					mdlx.ExportDAE(@"kh2\res\ram_exported\" + folder_names[i]+@"\"+ model_filename + ".dae");
				}
				catch
				{
					Console.WriteLine("COMMENT THE TEST 'RunSoraikoTest' to ignore the current test.");
					Console.WriteLine("Something went wrong. Make sure that there are models to dump in the RAM of PCSX2.");
					Console.WriteLine("Otherwise, my code needs improvement.");
				}

			}
		}

	}
}
