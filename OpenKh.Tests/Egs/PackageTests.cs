using OpenKh.Egs;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Egs
{
    public class PackageTests
    {
        private static string ToString(byte[] array) => string.Join(" ", array.Select(x => x.ToString("X02")));
        
        [Fact]
        public void GenerateKeyCorrectly()
        {
            byte[] expected = new byte[]
            {
                0x90, 0x94, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0xF0, 0x2D, 0x0A, 0x0B, 0x83, 0x1E, 0x4E, 0x60,
                0x1F, 0x61, 0x9A, 0xEE, 0x1B, 0x64, 0x9C, 0xE9, 0xEB, 0x49, 0x96, 0xE2, 0x68, 0x57, 0xD8, 0x82,
                0xF5, 0x54, 0xEF, 0xB1, 0xEE, 0x30, 0x73, 0x58, 0x05, 0x79, 0xE5, 0xBA, 0x6D, 0x2E, 0x3D, 0x38,
                0xBF, 0x22, 0x88, 0xAC, 0x51, 0x12, 0xFB, 0xF4, 0x54, 0x6B, 0x1E, 0x4E, 0x39, 0x45, 0x23, 0x76,
                0x27, 0xA3, 0xD1, 0x10, 0x76, 0xB1, 0x2A, 0xE4, 0x22, 0xDA, 0x34, 0xAA, 0x1B, 0x9F, 0x17, 0xDC,
                0xB2, 0x42, 0x70, 0x15, 0xC4, 0xF3, 0x5A, 0xF1, 0xE6, 0x29, 0x6E, 0x5B, 0xFD, 0xB6, 0x79, 0x87,
                0xAB, 0xD8, 0xF7, 0xE8, 0x6F, 0x2B, 0xAD, 0x19, 0x89, 0x02, 0xC3, 0x42, 0x74, 0xB4, 0xBA, 0xC5,
                0x2E, 0x80, 0x83, 0x89, 0x41, 0xAB, 0x2E, 0x90, 0xC8, 0xA9, 0xED, 0xD2, 0xBC, 0x1D, 0x57, 0x17,
                0x7C, 0x68, 0x62, 0xF5, 0x3D, 0xC3, 0x4C, 0x65, 0xF5, 0x6A, 0xA1, 0xB7, 0x49, 0x77, 0xF6, 0xA0,
                0x80, 0xA8, 0x27, 0x58, 0xBD, 0x6B, 0x6B, 0x3D, 0x48, 0x01, 0xCA, 0x8A, 0x01, 0x76, 0x3C, 0x2A,
                0xEF, 0x8F, 0x32, 0xD0, 0x52, 0xE4, 0x59, 0xED, 0x1A, 0xE5, 0x93, 0x67, 0x1B, 0x93, 0xAF, 0x4D,
            };

            var seed = new byte[]
            {
                0x90, 0x94, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x2D, 0x00, 0x00, 0x83, 0x1E, 0x4E, 0x60
            };
            Assert.Equal(ToString(expected), ToString(EgsEncryption.GenerateKey(seed, 10)));
        }

        [Fact]
        public void DecryptFirstChunkCorrectly()
        {
            byte[] encrypted = new byte[]
            {
                0x90, 0x94, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x2D, 0x00, 0x00, 0x83, 0x1E, 0x4E, 0x60,
                0xBE, 0xE9, 0xE8, 0x94, 0xE9, 0x51, 0x8B, 0x2E, 0x9E, 0xDF, 0x9E, 0x69, 0xCC, 0x8B, 0x55, 0x79
            };
            byte[] expected = new byte[]
            {
                0x78, 0x9C, 0xB5, 0x7D, 0x07, 0x9C, 0x14, 0x45, 0xF6, 0x7F, 0x75, 0x4F, 0x0E, 0xBB, 0x3B, 0x9B
            };

            var data = EgsEncryption.Decrypt(new MemoryStream(encrypted));
            Assert.Equal(ToString(expected), ToString(data[0..16]));
        }

        [Fact]
        public void UnpackCompressedAndEncryptedPkgFile()
        {
            EgsTools.Extract("Egs/res/dummy_compressed_encrypted.hed", "output", false);

            Assert.True(File.Exists("output/original/dummy.txt"));
            Assert.Equal("COUCOU", File.ReadAllText("output/original/dummy.txt"));

            Directory.Delete("output", true);
        }

        [Fact]
        public void UnpackCompressedPkgFile()
        {
            EgsTools.Extract("Egs/res/dummy_compressed.hed", "output", false);

            Assert.True(File.Exists("output/original/dummy.txt"));
            Assert.Equal("COUCOU", File.ReadAllText("output/original/dummy.txt"));

            Directory.Delete("output", true);
        }

        [Fact]
        public void UnpackPkgFile()
        {
            EgsTools.Extract("Egs/res/dummy.hed", "output", false);

            Assert.True(File.Exists("output/original/dummy.txt"));
            Assert.Equal("COUCOU", File.ReadAllText("output/original/dummy.txt"));

            Directory.Delete("output", true);
        }

        [Fact]
        public void PatchCompressedAndEncryptedPkgFile()
        {
            EgsTools.Patch("Egs/res/dummy_compressed_encrypted.pkg", "Egs/res/patch", "output");

            EgsTools.Extract("output/dummy_compressed_encrypted.hed", "output", false);

            Assert.True(File.Exists("output/original/dummy.txt"));
            Assert.Equal("COUCOU2", File.ReadAllText("output/original/dummy.txt"));

            Directory.Delete("output", true);
        }

        [Fact]
        public void PatchCompressedPkgFile()
        {
            EgsTools.Patch("Egs/res/dummy_compressed.pkg", "Egs/res/patch", "output");

            EgsTools.Extract("output/dummy_compressed.hed", "output", false);

            Assert.True(File.Exists("output/original/dummy.txt"));
            Assert.Equal("COUCOU2", File.ReadAllText("output/original/dummy.txt"));

            Directory.Delete("output", true);
        }

        [Fact]
        public void PatchPkgFile()
        {
            EgsTools.Patch("Egs/res/dummy.pkg", "Egs/res/patch", "output");

            EgsTools.Extract("output/dummy.hed", "output", false);

            Assert.True(File.Exists("output/original/dummy.txt"));
            Assert.Equal("COUCOU2", File.ReadAllText("output/original/dummy.txt"));

            Directory.Delete("output", true);
        }

        [Fact]
        public void AddNewAssetToPkgFile()
        {
            EgsTools.Patch("Egs/res/dummy.pkg", "Egs/res/newFile", "output");
            EgsTools.Extract("output/dummy.hed", "output", false);

            Assert.True(File.Exists("output/original/3099BDDF69006018C7C4385006B0B91D.dat"));
            Assert.Equal("This is a new file.", File.ReadAllText("output/original/3099BDDF69006018C7C4385006B0B91D.dat"));

            Directory.Delete("output", true);
        }

        // TODO: Write a test to add/patch remastered assets
    }
}
