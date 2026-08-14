using OpenKh.Kh1;
using System.IO;
using System.Text;
using Xunit;

namespace OpenKh.Tests.Kh1
{
    public class Kh1TextTableTests
    {
        private const string TableText =
            "00={eol}\n" +
            "01= \n" +
            "02={lf}\n" +
            "2B=A\n" +
            "2C=B\n" +
            "2D=C\n" +
            "45=a\n" +
            "68=.\n" +
            "94={icon}\n" +
            "95={icon}\n";

        [Fact]
        public void SpaceIs01AndZeroIsEol()
        {
            var table = ReadTable();

            Assert.Equal(new byte[] { 0x01 }, table.Encode(" "));
            Assert.Equal(new byte[] { 0x00 }, table.Encode("{eol}"));
            Assert.Equal(" ", table.Decode(new byte[] { 0x01 }));
            Assert.Equal("{eol}", table.Decode(new byte[] { 0x00 }));
        }

        [Fact]
        public void DefaultTableContainsTheInternationalKh1Encoding()
        {
            var table = Kh1TextTable.Default;

            Assert.Equal(new byte[] { 0x01 }, table.Encode(" "));
            Assert.Equal(new byte[] { 0xCB, 0x3B, 0x59, 0xE9, 0x60 }, table.Encode("¿Qué?"));
            Assert.Equal("ÁÉÍÓÚ áéíóú ñ", table.Decode(new byte[]
            {
                0xCD, 0xD2, 0xD6, 0xDB, 0xDF, 0x01,
                0xE4, 0xE9, 0xED, 0xF2, 0xF6, 0x01, 0xF0,
            }));
        }

        [Fact]
        public void DefaultTableRoundTripsEveryByte()
        {
            var table = Kh1TextTable.Default;

            for (var value = 0; value <= byte.MaxValue; value++)
            {
                var original = new[] { (byte)value };
                Assert.Equal(original, table.Encode(table.Decode(original)));
            }
        }

        [Fact]
        public void AmbiguousTextUsesRawTokensForLosslessRoundTrip()
        {
            var table = ReadTable();

            var decoded = table.Decode(new byte[] { 0x94, 0x95 });

            Assert.Equal("{0x94}{0x95}", decoded);
            Assert.Equal(new byte[] { 0x94, 0x95 }, table.Encode(decoded));
        }

        [Fact]
        public void BinlRoundTripsCommandsPaddingAndFalseRecordMarkers()
        {
            var table = ReadTable();
            var original = CreateBinl();
            using var input = new MemoryStream(original);

            var binl = Kh1Binl.Read(input, table);

            Assert.Equal("SP", binl.Language);
            Assert.Equal(2, binl.Entries.Count);
            Assert.Equal("A B\r\nC.", binl.Entries[0].Text);
            Assert.Equal("a{cmd:0C 04}A.", binl.Entries[1].Text);

            using var output = new MemoryStream();
            binl.Write(output);
            Assert.Equal(original, output.ToArray());
        }

        [Fact]
        public void BinlCanGrowAndBeReadAgain()
        {
            var table = ReadTable();
            using var input = new MemoryStream(CreateBinl());
            var binl = Kh1Binl.Read(input, table);
            binl.Entries[0].Text = "A B A\nC.";

            using var output = new MemoryStream();
            binl.Write(output);
            Assert.Equal(0, output.Length % 16);

            output.Position = 0;
            var reopened = Kh1Binl.Read(output, table);
            Assert.Equal("A B A\r\nC.", reopened.Entries[0].Text);
            Assert.Equal(binl.Entries.Count, reopened.Entries.Count);
        }

        private static Kh1TextTable ReadTable()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TableText));
            return Kh1TextTable.Read(stream);
        }

        private static byte[] CreateBinl()
        {
            using var stream = new MemoryStream();
            stream.Write(Encoding.ASCII.GetBytes("EvMsgSP"));
            stream.Write(new byte[] { 0x02, 0x00, 0x00, 0x00, 0x04 });

            stream.Write(new byte[]
            {
                0x0A, 0x00, 0x00, 0x00,
                0x07, 0x0C, 0x00,
                0x2B, 0x01, 0x2C, 0x02, 0x2D, 0x68,
                0x05, 0x6E, 0x00,
                0x0D, 0x01, 0x0A, 0x00,
                0x00,
            });
            stream.Write(new byte[]
            {
                0x0A, 0x00, 0x00, 0x00,
                0x07, 0x0C, 0x00,
                0x45, 0x0C, 0x04, 0x2B, 0x68,
                0x06, 0x3C, 0x00, 0x00, 0x08,
            });
            while ((stream.Length & 0x0F) != 0)
                stream.WriteByte(0xCD);
            return stream.ToArray();
        }
    }
}
