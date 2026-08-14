using OpenKh.Kh1;
using System;
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
            Assert.Equal(new byte[] { 0x0F }, table.Encode("{ctrl:0F}"));
            Assert.Equal("{ctrl:0F}", table.Decode(new byte[] { 0x0F }));
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

        [Fact]
        public void BinlIgnoresPresentationRecordsWithoutATextTerminator()
        {
            var table = ReadTable();
            var original = CreateBinl();
            using var stream = new MemoryStream();
            stream.Write(original, 0, 11);
            stream.Write(new byte[]
            {
                0x0A, 0x00, 0x00, 0x00,
                0x07, 0x0C, 0x00,
                0x0B, 0x00, 0x04, 0x00,
                0x81, 0x81, 0x81,
            });
            while ((stream.Length & 0x0F) != 0)
                stream.WriteByte(0xCD);
            stream.Position = 0;

            var binl = Kh1Binl.Read(stream, table);
            Assert.Empty(binl.Entries);

            using var output = new MemoryStream();
            binl.Write(output);
            Assert.Equal(stream.ToArray(), output.ToArray());
        }

        [Fact]
        public void KmbRoundTripsEntriesAndZeroPadding()
        {
            var table = ReadTable();
            var original = CreateKmb(0x00);
            using var input = new MemoryStream(original);

            var kmb = Kh1Kmb.Read(input, table);

            Assert.Equal(3, kmb.Entries.Count);
            Assert.Equal("A B", kmb.Entries[0].Text);
            Assert.Equal("a\r\nA", kmb.Entries[1].Text);
            Assert.Equal("{0x0F}", kmb.Entries[2].Text);

            using var output = new MemoryStream();
            kmb.Write(output);
            Assert.Equal(original, output.ToArray());
        }

        [Theory]
        [InlineData(0x00)]
        [InlineData(0xCD)]
        public void KmbCanGrowAndBeReadAgain(byte padding)
        {
            var table = ReadTable();
            using var input = new MemoryStream(CreateKmb(padding));
            var kmb = Kh1Kmb.Read(input, table);
            kmb.Entries[0].Text = "A B A B A B A B A B";

            using var output = new MemoryStream();
            kmb.Write(output);
            Assert.Equal(0, output.Length % 16);

            output.Position = 0;
            var reopened = Kh1Kmb.Read(output, table);
            Assert.Equal("A B A B A B A B A B", reopened.Entries[0].Text);
            Assert.Equal(3, reopened.Entries.Count);
        }

        [Fact]
        public void MessageV361RoundTripsAndCanGrow()
        {
            var table = ReadTable();
            var original = CreateMessageV361();
            using var input = new MemoryStream(original);
            var message = Kh1MessageV361.Read(input, table);

            Assert.Equal(2, message.Entries.Count);
            Assert.Equal("A", message.Entries[0].Text);
            Assert.Equal("a\r\nA", message.Entries[1].Text);

            using var unchanged = new MemoryStream();
            message.Write(unchanged);
            Assert.Equal(original, unchanged.ToArray());

            message.Entries[0].Text = "A B A B A B A B A B";
            using var edited = new MemoryStream();
            message.Write(edited);
            Assert.Equal(0, edited.Length % 16);
            edited.Position = 0;
            var reopened = Kh1MessageV361.Read(edited, table);
            Assert.Equal("A B A B A B A B A B", reopened.Entries[0].Text);
            Assert.Equal("a\r\nA", reopened.Entries[1].Text);
        }

        [Fact]
        public void TextBinRoundTripsAndCanGrow()
        {
            var table = ReadTable();
            var original = CreateTextBin();
            using var input = new MemoryStream(original);
            var textBin = Kh1TextBin.Read(input, table);

            Assert.Equal(2, textBin.Entries.Count);
            Assert.Equal("A B", textBin.Entries[0].Text);
            Assert.Equal("a\r\nA", textBin.Entries[1].Text);

            using var unchanged = new MemoryStream();
            textBin.Write(unchanged);
            Assert.Equal(original, unchanged.ToArray());

            textBin.Entries[0].Text = "A B A B A B";
            using var edited = new MemoryStream();
            textBin.Write(edited);
            Assert.Equal(0, edited.Length % 16);
            edited.Position = 0;
            var reopened = Kh1TextBin.Read(edited, table);
            Assert.Equal("A B A B A B", reopened.Entries[0].Text);
            Assert.Equal("a\r\nA", reopened.Entries[1].Text);
        }

        [Fact]
        public void EventMessageRoundTripsGrowsAndRelocatesOffsets()
        {
            var table = ReadTable();
            var original = CreateEventMessage();
            using var input = new MemoryStream(original);
            var message = Kh1EventMessage.Read(input, table);

            Assert.Equal(2, message.Entries.Count);
            Assert.Equal("A B", message.Entries[0].Text);
            Assert.Equal("a A", message.Entries[1].Text);

            using var unchanged = new MemoryStream();
            message.Write(unchanged);
            Assert.Equal(original, unchanged.ToArray());

            var oldFirstBoundary = BitConverter.ToUInt32(original, 0x10);
            var oldSecondBoundary = BitConverter.ToUInt32(original, 0x14);
            message.Entries[0].Text = "A B A B A B A B A B A B A B A B";
            using var edited = new MemoryStream();
            message.Write(edited);
            var editedBytes = edited.ToArray();
            var delta = BitConverter.ToUInt32(editedBytes, 0x10) - oldFirstBoundary;
            Assert.True(delta > 0);
            Assert.Equal(oldSecondBoundary + delta, BitConverter.ToUInt32(editedBytes, 0x14));

            edited.Position = 0;
            var reopened = Kh1EventMessage.Read(edited, table);
            Assert.Equal(message.Entries[0].Text, reopened.Entries[0].Text);
            Assert.Equal("a A", reopened.Entries[1].Text);
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

        private static byte[] CreateKmb(byte padding)
        {
            using var stream = new MemoryStream();
            stream.Write(BitConverter.GetBytes(3));
            stream.Write(new byte[] { 0x2B, 0x01, 0x2C, 0x00 });
            stream.Write(new byte[] { 0x45, 0x02, 0x2B, 0x00 });
            stream.Write(new byte[] { 0x0F, 0x00 });
            while ((stream.Length & 0x0F) != 0)
                stream.WriteByte(padding);
            return stream.ToArray();
        }

        private static byte[] CreateMessageV361()
        {
            using var stream = new MemoryStream();
            stream.Write(Encoding.ASCII.GetBytes("Message v361"));
            stream.Write(BitConverter.GetBytes(2));
            stream.Write(BitConverter.GetBytes(0x20));
            stream.Write(BitConverter.GetBytes(0x26));
            stream.Write(BitConverter.GetBytes(0x06));
            stream.Write(BitConverter.GetBytes(0x07));
            stream.Write(new byte[] { 0x00, 0x00, 0x02, 0x00, 0x06, 0x00 });
            stream.Write(new byte[] { 0x2B, 0x00, 0x45, 0x02, 0x2B, 0x00, 0x00 });
            while ((stream.Length & 0x0F) != 0)
                stream.WriteByte(0xCD);
            return stream.ToArray();
        }

        private static byte[] CreateTextBin()
        {
            using var stream = new MemoryStream();
            stream.Write(new byte[] { 0x2B, 0x01, 0x2C, 0x00 });
            stream.Write(new byte[] { 0x45, 0x02, 0x2B, 0x00 });
            while ((stream.Length & 0x0F) != 0)
                stream.WriteByte(0x00);
            return stream.ToArray();
        }

        private static byte[] CreateEventMessage()
        {
            using var section = new MemoryStream();
            section.Write(BitConverter.GetBytes(2));
            section.Write(new byte[]
            {
                0x0A, 0x00, 0x00, 0x00,
                0x07, 0x0C, 0x00,
                0x2B, 0x01, 0x2C,
                0x05, 0x10, 0x00,
                0x0A, 0x00, 0x00, 0x00,
                0x07, 0x0C, 0x00,
                0x45, 0x01, 0x2B,
                0x06, 0x20, 0x00,
            });
            while ((section.Length & 0x0F) != 0)
                section.WriteByte(0x00);

            const int sectionStart = 0x18;
            var sectionEnd = sectionStart + checked((int)section.Length);
            var secondBoundary = sectionEnd + 0x10;
            using var stream = new MemoryStream();
            stream.Write(new byte[0x0C]);
            stream.Write(BitConverter.GetBytes(sectionStart));
            stream.Write(BitConverter.GetBytes(sectionEnd));
            stream.Write(BitConverter.GetBytes(secondBoundary));
            section.Position = 0;
            section.CopyTo(stream);
            stream.Write(new byte[0x20]);
            return stream.ToArray();
        }
    }
}
