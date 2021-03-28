using OpenKh.Command.TexFooter.Subcommands;
using OpenKh.Common;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace OpenKh.Tests.Commands
{
    public class TexFooterTests
    {
        [Theory]
        [InlineData("res/texfooter/clamp.map")]
        [InlineData("res/texfooter/clamp-uvsc.map")]
        [InlineData("res/texfooter/clamp-texa.map")]
        public void ShouldWriteBackTheExactSameFile(string mapFile)
        {
            var baseDir = Environment.CurrentDirectory;
            var outDir = Path.Combine(
                baseDir,
                nameof(TexFooterTests),
                Path.GetFileName(mapFile).Replace('.', '_')
            );
            Directory.CreateDirectory(outDir);

            var destWorkFile = Path.Combine(outDir, Path.GetFileName(mapFile));

            File.Copy(mapFile, destWorkFile, true);

            var exportErrorCode = new ExportCommand
            {
                MapFile = destWorkFile,
            }
                .Execute();

            if (exportErrorCode == 1)
            {
                // Having no texture data.
                return;
            }

            Assert.Equal(
                expected: 0,
                actual: exportErrorCode
            );

            Assert.Equal(
                expected: 0,
                actual:
                    new ImportCommand
                    {
                        MapFile = destWorkFile,
                    }
                        .Execute()
            );

            Assert.Equal(
                expected: FromFile(mapFile),
                actual: FromFile(destWorkFile)
            );
        }

        private IEnumerable<byte> FromFile(string file) => File.ReadAllBytes(file);
    }
}
