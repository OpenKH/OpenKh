using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Tools.Common;
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


			var process = ProcessStream.TryGetProcess(x => x.ProcessName == "pcsx2"); /* For RAM-related tests. */
			using var stream = new ProcessStream(process, 0x20000000, 0x2000000);

			//stream.Position = 0x00964c10;
			//var dpd = new OpenKh.Kh2.Dpd(stream);
			//var dpd = new OpenKh.Kh2.Dpd(File.OpenRead(@"D:\Hacking\KH2\reverse\SAMPLES\tt_0.dpd"));
			//var dpd = new OpenKh.Kh2.Dpd(File.OpenRead(@"D:\Hacking\KH2\reverse\SAMPLES\texcommon.dpd"));


		}
	}
}
