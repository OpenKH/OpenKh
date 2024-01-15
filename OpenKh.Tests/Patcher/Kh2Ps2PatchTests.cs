using OpenKh.Patcher.Kh2Ps2Patch;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace OpenKh.Tests.Patcher
{
    public class Kh2Ps2PatchTests
    {
        private readonly PatchIO _patchIo = new PatchIO();
        private readonly PatchIOHelper _patchIoHelper = new PatchIOHelper();
        private readonly PatchCodec _patchCodec = new PatchCodec();

        [Fact]
        public void TestGMethod()
        {
            var bytes = Enumerable.Range(0, 255).Select(it => (byte)it).ToArray();
            var encoded = _patchCodec.ApplyGovanifYsMethod(bytes);
            var decoded = _patchCodec.ApplyGovanifYsMethod(encoded);
            Assert.Equal(expected: bytes, actual: decoded);
        }

        [Fact]
        public void TestXMethod()
        {
            var bytes = Enumerable.Range(0, 255).Select(it => (byte)it).ToArray();
            var encoded = _patchCodec.ApplyXeeynamosMethod(bytes);
            var decoded = _patchCodec.ApplyXeeynamosMethod(encoded);
            Assert.Equal(expected: bytes, actual: decoded);
        }

        [Fact]
        public void TestPatchIO()
        {
            var raw = File.ReadAllBytes("Patcher/res/dummy.kh2patch");
            var patch1 = _patchIo.Read(raw);
            var reRaw1 = _patchIo.Write(patch1);
            var rePatch1 = _patchIo.Read(reRaw1);

            var xForm = _patchCodec.ApplyXeeynamosMethod(raw);
            var gForm = _patchCodec.ApplyGovanifYsMethod(raw);

            var patchReadByHelper = _patchIoHelper.Read(raw);
            var patchGReadByHelper = _patchIoHelper.Read(gForm);
            var patchXReadByHelper = _patchIoHelper.Read(xForm);

            foreach (var patch in new[] { patch1, rePatch1, patchReadByHelper, patchGReadByHelper, patchXReadByHelper })
            {
                //Press enter to run using the file: output.kh2patch
                //Enter author's name: author
                //Enter revision number: 1234567890
                //Enter changelog lines here (leave blank to continue):
                //cl1
                //cl2
                //cl3

                //Enter credits here (leave blank to continue):
                //cr1
                //cr2
                //cr3

                //Other information (leave blank to continue): other

                //Filenames may be formatted as text (msg/jp/lk.bar) or hash (0x030b45da).

                Assert.Equal(expected: "author", actual: patch.Author);
                Assert.Equal(expected: 1234567890U, actual: patch.Revision);
                Assert.Equal(expected: new string[] { "cl1", "cl2", "cl3" }, actual: patch.ChangeLogs);
                Assert.Equal(expected: new string[] { "cr1", "cr2", "cr3" }, actual: patch.Credits);
                Assert.Equal(expected: "other", actual: patch.OtherInformation);
                Assert.Equal(expected: 3, actual: patch.PatchEntries.Count());

                //Enter filename: file1.txt
                // Warning: Filename not found into the Hashlist.
                //  Using "file1.txt" for F3006671
                //Relink to this filename(ex: 000al.idx) [Blank for none]:
                //Too small to compress. Press enter.

                //Parent compressed file [Leave blank for KH2]:
                //  Using "KH2" for 00000000
                //Should this file be added if he's not in the game? [y/N] y

                Assert.Equal(expected: 5U, actual: patch.PatchEntries[0].CompressedSize);
                Assert.Equal(expected: 4076889713U, actual: patch.PatchEntries[0].Hash);
                Assert.False(patch.PatchEntries[0].IsCompressed);
                Assert.True(patch.PatchEntries[0].IsCustomFile);
                Assert.Equal(expected: 0U, actual: patch.PatchEntries[0].Parent);
                Assert.Equal(expected: "file1", actual: Encoding.Latin1.GetString(patch.PatchEntries[0].RawContent.Span));
                Assert.Equal(expected: 0U, actual: patch.PatchEntries[0].Relink);
                Assert.Equal(expected: 5U, actual: patch.PatchEntries[0].UncompressedSize);

                //Enter filename: file2.txt
                // Warning: Filename not found into the Hashlist.
                //  Using "file2.txt" for 2817CEE6
                //Relink to this filename(ex: 000al.idx) [Blank for none]:
                //Too small to compress. Press enter.

                //Parent compressed file [Leave blank for KH2]:
                //  Using "KH2" for 00000000
                //Should this file be added if he's not in the game? [y/N] n

                Assert.Equal(expected: 5U, actual: patch.PatchEntries[1].CompressedSize);
                Assert.Equal(expected: 672648934U, actual: patch.PatchEntries[1].Hash);
                Assert.False(patch.PatchEntries[1].IsCompressed);
                Assert.False(patch.PatchEntries[1].IsCustomFile);
                Assert.Equal(expected: 0U, actual: patch.PatchEntries[1].Parent);
                Assert.Equal(expected: "file2", actual: Encoding.Latin1.GetString(patch.PatchEntries[1].RawContent.Span));
                Assert.Equal(expected: 0U, actual: patch.PatchEntries[1].Relink);
                Assert.Equal(expected: 5U, actual: patch.PatchEntries[1].UncompressedSize);

                //Enter filename: file3.txt
                // Warning: Filename not found into the Hashlist.
                //  Using "file3.txt" for 611AA96B
                //Relink to this filename(ex: 000al.idx) [Blank for none]: 000al.idx
                //  Using "000al.idx" for 35F6154A
                //Parent compressed file [Leave blank for KH2]:
                //  Using "KH2" for 00000000
                //Should this file be added if he's not in the game? [y/N] n

                Assert.Equal(expected: 0U, actual: patch.PatchEntries[2].CompressedSize);
                Assert.Equal(expected: 1629137259U, actual: patch.PatchEntries[2].Hash);
                Assert.False(patch.PatchEntries[2].IsCompressed);
                Assert.False(patch.PatchEntries[2].IsCustomFile);
                Assert.Equal(expected: 0U, actual: patch.PatchEntries[2].Parent);
                Assert.Equal(expected: "", actual: Encoding.Latin1.GetString(patch.PatchEntries[2].RawContent.Span));
                Assert.Equal(expected: 905319754U, actual: patch.PatchEntries[2].Relink); // 000al.idx
                Assert.Equal(expected: 0U, actual: patch.PatchEntries[2].UncompressedSize);
            }
        }
    }
}
