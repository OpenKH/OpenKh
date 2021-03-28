using OpenKh.Common;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class IdxTests
    {
        [Theory]
        [InlineData("test", 0x338BCFAC)]
        [InlineData("hello world!", 0xFD8495B7)]
        [InlineData("00battle.bin", 0x2029C445)]
        public void CalculateHash32(string text, uint hash)
        {
            Assert.Equal(hash, Idx.GetHash32(text));
        }

        [Theory]
        [InlineData("test", 0xB82F)]
        [InlineData("hello world!", 0x0907)]
        public void CalculateHash16(string text, ushort hash)
        {
            Assert.Equal(hash, Idx.GetHash16(text));
        }

        [Fact]
        public void ReadIdxEntry()
        {
            var entry = MockIdxEntry(1, 2, 3, 4, 5);
            Assert.Equal(1U, entry.Hash32);
            Assert.Equal(2, entry.Hash16);
            Assert.Equal(3, entry.BlockLength);
            Assert.Equal(4U, entry.Offset);
            Assert.Equal(5, entry.Length);
        }

        [Fact]
        public void WriteIdxEntry() => Helpers.AssertStream(CreateMockIdxStream(1, 2, 3, 4, 5), stream =>
        {
            var outStream = new MemoryStream();
            Idx.Write(outStream, Idx.Read(stream));

            return outStream;
        });

        [Theory]
        [InlineData(0x3fff, false, false)]
        [InlineData(0x4000, true, false)]
        [InlineData(0x8000, false, true)]
        [InlineData(0xc000, true, true)]
        public void ReadBlockDescriptionFlags(ushort blockDescriptor, bool expectedIsCompressed, bool expectedIsStreamed)
        {
            var entry = MockIdxEntry(0, 0, blockDescriptor, 0, 0);
            Assert.Equal(expectedIsCompressed, entry.IsCompressed);
            Assert.Equal(expectedIsStreamed, entry.IsStreamed);
        }

        /// <summary>
        /// This test addresses a rare bug regarding the file obj/B_LK120_RAW.mset,
        /// where its block size is stored as 0x710 rather than 0x1710.
        /// It is the only known file that have this behaviour.
        /// </summary>
        [Theory]
        [InlineData(0x4710, 0x13EFFF0, 0x1710)] // the scenario on any KH2.IDX
        [InlineData(0x0710, 0x13EFFF0, 0x0710)] // do not break other stuff
        [InlineData(0x4710, 0xFFFFFF, 0x0710)] // do not break other stuff
        [InlineData(0x5710, 0x13EFFF0, 0x1710)] // do not break it if it is fixed
        public void IdxBlockSizeBugTest(ushort blockDescriptor, int uncompressedLength, int expectedBlockCount)
        {
            var entry = MockIdxEntry(0, 0, blockDescriptor, 0, uncompressedLength);
            Assert.Equal(expectedBlockCount, (int)entry.BlockLength);
        }

        [Theory]
        [InlineData(0x2029C445, 0x176F, "00battle.bin")]
        [InlineData(0x10303F6F, 0xF325, "obj/B_LK120_RAW.mset")]
        [InlineData(0x01234567, 0xcdef, null)]
        public void GiveRealNames(uint hash32, ushort hash16, string name)
        {
            Assert.Equal(name, IdxName.Lookup(hash32, hash16));
        }

        private static Stream CreateMockIdxStream(uint hash32, ushort hash16, ushort blockDescriptor, int offset, int length)
        {
            const int IdxFileCount = 1;

            var stream = new MemoryStream(0x14);
            stream.Write(IdxFileCount);
            stream.Write(hash32);
            stream.Write(hash16);
            stream.Write(blockDescriptor);
            stream.Write(offset);
            stream.Write(length);

            return stream.SetPosition(0);
        }

        private static List<Idx.Entry> MockIdx(uint hash32, ushort hash16, ushort blockDescriptor, int offset, int length) =>
            CreateMockIdxStream(hash32, hash16, blockDescriptor, offset, length).Using(Idx.Read);

        private static Idx.Entry MockIdxEntry(uint hash32, ushort hash16, ushort blockDescriptor, int offset, int length) =>
            MockIdx(hash32, hash16, blockDescriptor, offset, length).First();
    }
}
