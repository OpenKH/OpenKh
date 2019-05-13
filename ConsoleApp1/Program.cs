using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
	class Program
	{
		static void Main(string[] args)
		{
			//var stream = ProcessStream.SearchProcess("pcsx2");
			//stream.Position = 0x00964c10;
			//var dpd = new OpenKh.Kh2.Dpd(stream);

			//var dpd = new OpenKh.Kh2.Dpd(File.OpenRead(@"D:\Hacking\KH2\reverse\SAMPLES\tt_0.dpd"));
			var dpd = new OpenKh.Kh2.Dpd(File.OpenRead(@"D:\Hacking\KH2\reverse\SAMPLES\texcommon.dpd"));
		}
	}
}
