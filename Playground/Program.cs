using OpenKh.Common;
using OpenKh.Kh1;
using OpenKh.Kh2;
using OpenKh.Bbs;
using OpenKh.Tools.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenKh.Tools.Common.Imaging;

const string Input = @"D:\kh2\somefile.bar";
const string Output = @"D:\kh2\modifiedfile.bar";

Console.WriteLine("Hello OpenKH!");

var idxs = File.OpenRead(@"E:\Hacking\KH1\KINGDOM.IDX").Using(Idx1.Read);
var names = Idx1Name.Lookup(idxs.OrderBy(x => x.IsoBlock)).OrderBy(x => x.Entry.Hash).ToList();
File.WriteAllLines(@"D:\asd.txt", names.Select(x => x.ToString()));
var aaa = File
    .ReadAllLines(Path.Combine(AppContext.BaseDirectory, "resources/kh1idx.txt"))
    .Select(x => new
    {
        Name = x,
        Hash = Idx1.GetHash(x)
    })
    .GroupBy(x => x.Hash)
    .Where(x => x.Count() > 1)
    .Select(x => new
    {
        Name1 = x.First().Name,
        Name2 = x.Skip(1).First().Name
    })
    .ToList();
idxs = idxs;
// Parse, modify and save a KH2 BinArc file:
//var binarc = File.OpenRead(Input).Using(Bar.Read);
//// perform here your changes
//File.Create(Output).Using(stream => Bar.Write(stream, binarc));

// Parse and convert all textures to PNG from a BBS 3D model using all CPUs:
//var pmo = File.OpenRead(Input).Using(Pmo.Read);
//pmo.texturesData
//    .Select((texture, index) => new { texture, index })
//    .AsParallel()
//    .ForAll(x => x.texture.SaveImage($"{pmo.textureInfo[x.index].TextureName}.png"));

// Connect to PCSX2 and do live editing via code:
//var process = ProcessStream.TryGetProcess(x => x.ProcessName == "pcsx2");
//using var stream = new ProcessStream(process, 0x20000000, 0x2000000);
//var gameSpeed = stream.SetPosition(0x349E0C).ReadFloat();
//stream.SetPosition(0x349E0C).Write(gameSpeed);
