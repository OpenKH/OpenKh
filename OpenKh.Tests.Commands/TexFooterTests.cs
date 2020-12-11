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
        private static string assetMapDir = @"H:\KH2fm.OpenKH\map\jp";

        public static IEnumerable<object[]> IncludeAssetMapFiles() =>
            Directory.GetFiles(assetMapDir, "*.map")
                .Select(file => new object[] { file });

        private static string assetObjDir = @"H:\KH2fm.OpenKH\obj";

        public static IEnumerable<object[]> IncludeAssetModelFiles() =>
            Directory.GetFiles(assetObjDir, "*.mdlx")
                .Select(file => new object[] { file });

        //[Theory]
        //[MemberData(nameof(IncludeAssetMapFiles))]
        //[MemberData(nameof(IncludeAssetModelFiles))]
        public void ShouldWriteBackTheExactSameFile(string mapOrMdlxFile)
        {
            var baseDir = Environment.CurrentDirectory;
            var outDir = Path.Combine(
                baseDir,
                nameof(TexFooterTests),
                Path.GetFileName(mapOrMdlxFile).Replace('.', '_')
            );
            Directory.CreateDirectory(outDir);

            var destWorkFile = Path.Combine(outDir, Path.GetFileName(mapOrMdlxFile));

            File.Copy(mapOrMdlxFile, destWorkFile, true);

            var exportErrorCode = new ExportCommand
            {
                MapFile = destWorkFile,
            }
                .OnExecute();

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
                        .OnExecute()
            );

            Assert.Equal(
                expected: ExtractTextureFooterDataFromFile(mapOrMdlxFile),
                actual: ExtractTextureFooterDataFromFile(destWorkFile)
            );
        }

        private IEnumerable<byte> ExtractTextureFooterDataFromFile(string mapOrMdlxFile)
        {
            var barEntries = File.OpenRead(mapOrMdlxFile).Using(Bar.Read);
            var modelTextureEntry = barEntries.FirstOrDefault(it => it.Type == Bar.EntryType.ModelTexture);
            if (modelTextureEntry != null)
            {
                var modelTexture = ModelTexture.Read(modelTextureEntry.Stream);
                var footerData = modelTexture.GetFooterData();
                if (footerData != null)
                {
                    return footerData;
                }
            }
            return new byte[0];
        }
    }
}
