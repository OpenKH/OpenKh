using OpenKh.Command.Bdxio.Models;
using OpenKh.Command.Bdxio.Utils;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenKh.Tests.Commands
{
    public class BdxRegressionTests
    {
        private static string SourceRootDir => Path.Combine(Environment.CurrentDirectory, "res", "bdx");

        private const bool RegenerateTestResults = false;

        [Theory]
        [MemberData(nameof(GetSource))]
        public void TestSamples(string sourceDir)
        {
            TestSamplesWith(sourceDir, CultureInfo.InvariantCulture);
        }

        [Theory]
        [MemberData(nameof(GetSource))]
        public void TestSamplesFrench(string sourceDir)
        {
            TestSamplesWith(sourceDir, CultureInfo.GetCultureInfo("fr-FR"));
        }

        private void TestSamplesWith(string sourceDir, CultureInfo cultureInfo)
        {
            Console.WriteLine(sourceDir);

            CultureInfo.CurrentCulture = cultureInfo;

            var InputFile = Path.Combine(sourceDir, "a.bdscript");
            var outFile = Path.Combine(sourceDir, "a.bdx");

            var ascii = BdxAsciiModel.ParseText(File.ReadAllText(InputFile));
            var encoder = new BdxEncoder(
                header: new YamlDotNet.Serialization.DeserializerBuilder()
                    .Build()
                    .Deserialize<BdxHeader>(
                        ascii.Header ?? ""
                    ),
                script: ascii.GetLineNumberRetainedScriptBody(),
                scriptName: InputFile,
                loadScript: fileName => File.ReadAllText(
                    Path.Combine(
                        Path.GetDirectoryName(InputFile) ?? ".",
                        fileName
                    )
                )
            );

            if (RegenerateTestResults)
            {
                File.WriteAllBytes(
                    outFile,
                    encoder.Content
                );
            }
            else
            {
                Assert.Equal(
                    expected: File.ReadAllBytes(outFile),
                    actual: encoder.Content
                );
            }

            var decoded = new BdxDecoder(new MemoryStream(encoder.Content, false));
            var bdscript = BdxDecoder.TextFormatter.Format(decoded);

            var reBdscript = Path.Combine(sourceDir, "b.bdscript");

            if (RegenerateTestResults)
            {
                File.WriteAllText(
                    reBdscript,
                    bdscript
                );
            }
            else
            {
                Assert.Equal(
                    expected: File.ReadAllText(reBdscript),
                    actual: bdscript
                );
            }
        }

        public static IEnumerable<object[]> GetSource()
        {
            foreach (var dir in Directory.GetDirectories(SourceRootDir))
            {
                yield return new object[] { dir };
            }
        }
    }
}
