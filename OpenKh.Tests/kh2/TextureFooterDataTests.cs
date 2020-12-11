using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.TextureFooter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class TextureFooterDataTests
    {
        private static string assetMapDir = @"H:\KH2fm.OpenKH\map\jp";

        public static IEnumerable<object[]> IncludeAssetMapFiles() =>
            Directory.GetFiles(assetMapDir, "*.map")
                .Select(file => new object[] { file, false });

        private static string assetObjDir = @"H:\KH2fm.OpenKH\obj";

        public static IEnumerable<object[]> IncludeAssetModelFiles() =>
            Directory.GetFiles(assetObjDir, "*.mdlx")
                .Select(file => new object[] { file, true });

        //[Theory]
        //[MemberData(nameof(IncludeAssetMapFiles))]
        //[MemberData(nameof(IncludeAssetModelFiles))]
        public void ShouldWriteBackTheExactSameFile(string mapOrMdlxFile, bool thisIsMdlx)
        {
            File.OpenRead(mapOrMdlxFile).Using(
                stream =>
                {
                    var barEntries = Bar.Read(stream);

                    {
                        var modelEntry = barEntries.FirstOrDefault(it => it.Type == Bar.EntryType.Model);

                        if (modelEntry != null)
                        {
                            var mdlx = Mdlx.Read(modelEntry.Stream);

                            if (thisIsMdlx)
                            {
                                Assert.False(mdlx.IsMap, "It has to be a model file.");
                            }
                            else
                            {
                                Assert.True(mdlx.IsMap, "It has to be a map file.");
                            }
                        }
                    }

                    var modelTextureEntry = barEntries.FirstOrDefault(it => it.Type == Bar.EntryType.ModelTexture);
                    if (modelTextureEntry == null || modelTextureEntry.Stream.Length == 0)
                    {
                        // Having no texture data
                        return;
                    }

                    var modelTexture = ModelTexture.Read(modelTextureEntry.Stream);

                    var footerDataStream = new MemoryStream(modelTexture.GetFooterData());

                    Helpers.AssertStream(
                        footerDataStream,
                        inStream =>
                        {
                            var textureFooterData = TextureFooterData.Read(inStream);

                            var outStream = new MemoryStream();
                            textureFooterData.Write(outStream);

                            return outStream;
                        }
                    );
                }
            );
        }
    }
}
