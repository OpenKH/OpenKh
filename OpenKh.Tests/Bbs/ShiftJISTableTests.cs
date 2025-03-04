using OpenKh.Bbs.Messages.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenKh.Tests.Bbs
{
    public class ShiftJISTableTests
    {
        [Theory]
        [InlineData('　', 0x8140)]
        [InlineData('、', 0x8141)]
        [InlineData('¶', 0x81F7)]
        [InlineData('０', 0x824F)]
        [InlineData('ん', 0x82F1)]
        [InlineData('北', 0x966B)]
        [InlineData('犾', 0xEE40)]
        [InlineData('鸙', 0xEEEB)]
        public void RegressionTests(char ch, ushort expectedCode)
        {
            var sjisCode = ShiftJISTable.TryToEncodeToShiftJISCharacter(ch);
            Assert.NotNull(sjisCode);
            Assert.Equal(expectedCode.ToString("X04"), sjisCode.Value.ToString("X04"));
            var firstByte = (byte)(sjisCode.Value >> 8);
            var secondByte = (byte)(sjisCode.Value);
            Assert.True(ShiftJISTable.IsLeadingByteOfShiftJIS(firstByte, out int lineIndex));
            var recoveredCh = ShiftJISTable.GetShiftJISCharFromTrailingByte(secondByte, lineIndex);
            Assert.Equal(ch, recoveredCh);
        }
    }
}
