using OpenKh.Command.Bdxio.Utils;
using OpenKh.Common;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenKh.Tests.Commands
{
    public class BdxMassiveTests
    {
        private static string SourceDir => Environment.GetEnvironmentVariable("KH2FM_EXTRACTION_DIR");

        //[Theory(Skip = "Only for local development")]
        [Theory]
        [MemberData(nameof(GetSource))]
        public void RegressionTestSameLengthSameScript(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return;
            }

            Console.WriteLine(source);

            var outDir = Path.Combine(
                Environment.CurrentDirectory,
                Path.GetFileNameWithoutExtension(source.Split('\t').First())
            );
            Directory.CreateDirectory(outDir);

            BdxDecoder DecodeBdx(byte[] bdx)
            {
                return new BdxDecoder(
                    new MemoryStream(bdx, false),
                    codeRevealer: true,
                    codeRevealerLabeling: true
                );
            }

            var bdx = ResolveSource(source);
            var decoded = DecodeBdx(bdx);
            var bdscript = BdxDecoder.TextFormatter.Format(decoded);
            var model = BdxAsciiModel.ParseText(bdscript);

            File.WriteAllBytes(
                Path.Combine(outDir, "a.bdx"),
                bdx
            );
            File.WriteAllText(
                Path.Combine(outDir, "a.bdscript"),
                bdscript
            );

            var aBdscript = Path.Combine(outDir, "a.bdscript");

            var encoded = new BdxEncoder(
                header: decoded.Header,
                script: model.GetLineNumberRetainedScriptBody(),
                scriptName: aBdscript,
                loadScript: fileName => File.ReadAllText(
                    Path.Combine(
                        Path.GetDirectoryName(outDir) ?? ".",
                        fileName
                    )
                )
            );

            var newBdx = encoded.Content;

            File.WriteAllBytes(
                Path.Combine(outDir, "b.bdx"),
                newBdx
            );

            // Header data must be equal
            Assert.Equal(
                expected: bdx.Take(16 + 4 + 4 + 4).ToArray(),
                actual: newBdx.Take(16 + 4 + 4 + 4).ToArray()
            );

            Assert.Equal(
                expected: bdx.Length,
                actual: newBdx.Length
            );

            {
                var reDecoded = DecodeBdx(encoded.Content);
                var reBdscript = BdxDecoder.TextFormatter.Format(reDecoded);

                File.WriteAllText(
                    Path.Combine(outDir, "b.bdscript"),
                    reBdscript
                );

                Assert.Equal(expected: bdscript, actual: reBdscript);
            }

            Assert.Equal(expected: bdx.Length, actual: encoded.Content.Length);
        }

        private byte[] ResolveSource(string source)
        {
            var sources = source.Split('\t');
            var bar = File.OpenRead(sources[0]).Using(fs => Bar.Read(fs));
            for (int x = 1; x < sources.Length; x++)
            {
                if (sources[x].StartsWith("#"))
                {
                    var idx = int.Parse(sources[x].Substring(1));

                    var temp = new MemoryStream();
                    bar[idx].Stream.CopyTo(temp);
                    return temp.ToArray();
                }
                else
                {
                    throw new InvalidDataException();
                }
            }
            throw new InvalidDataException();
        }

        public static IEnumerable<object[]> GetSource()
        {
            var srcDir = SourceDir;

            if (srcDir != null)
            {
                foreach (var barFile in Directory.GetFiles(Path.Combine(srcDir, "obj"), "*.mdlx"))
                {
                    foreach (var (barEntry, index) in File.OpenRead(barFile)
                        .Using(fs => Bar.Read(fs))
                        .Select((barEntry, index) => (barEntry, index))
                        .ToArray()
                        .Where(tuple => tuple.barEntry.Type == Bar.EntryType.Bdx)
                    )
                    {
                        yield return new object[] { $"{barFile}\t#{index}" };
                    }
                }
            }

            yield return new object[] { "" };
        }

        public static IEnumerable<object[]> GetSource1()
        {
            var temp = new MemoryStream();
            var path = Path.Combine(SourceDir, @"obj\B_EX100.mdlx");
            File.OpenRead(path).Using(fs => Bar.Read(fs)).Single(it => it.Type == Bar.EntryType.Bdx).Stream.CopyTo(temp);
            yield return new object[] { path, temp.ToArray(), };
        }
    }
}
