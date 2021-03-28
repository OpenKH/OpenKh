using OpenKh.Common.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace OpenKh.Tests.Common
{

    public class BitsUtilTest
    {
        [Fact]
        public void IntTests()
        {
            for (var index = 0; index < 32; index++)
            {
                Assert.True(BitsUtil.Int.GetBit(-1, index));
                Assert.False(BitsUtil.Int.GetBit(0, index));

                Assert.Equal(1, BitsUtil.Int.GetBits(-1, index, 1));
                Assert.Equal(0, BitsUtil.Int.GetBits(0, index, 1));
            }

            Assert.Equal(1, BitsUtil.Int.SetBit(0, 0, true));
            Assert.Equal(2, BitsUtil.Int.SetBit(0, 1, true));
            Assert.Equal(4, BitsUtil.Int.SetBit(0, 2, true));
            Assert.Equal(8, BitsUtil.Int.SetBit(0, 3, true));

            Assert.Equal(0x10000000, BitsUtil.Int.SetBit(0, 28, true));
            Assert.Equal(0x20000000, BitsUtil.Int.SetBit(0, 29, true));
            Assert.Equal(0x40000000, BitsUtil.Int.SetBit(0, 30, true));
            Assert.Equal(0x80000000U, (uint)BitsUtil.Int.SetBit(0, 31, true));

            Assert.Equal((10 & 15) << 2, BitsUtil.Int.SetBits(0, 2, 4, 10));
            Assert.Equal((26 & 15) << 2, BitsUtil.Int.SetBits(0, 2, 4, 26));

            Assert.Equal(0x000, BitsUtil.Int.SignExtend(0x000, 0, 11));
            Assert.Equal(0x001, BitsUtil.Int.SignExtend(0x001, 0, 11));
            Assert.Equal(0x7FE, BitsUtil.Int.SignExtend(0x7FE, 0, 11));
            Assert.Equal(0x7FF, BitsUtil.Int.SignExtend(0x7FF, 0, 11));
            Assert.Equal(-0x800, BitsUtil.Int.SignExtend(0x800, 0, 11));
            Assert.Equal(-0x7FF, BitsUtil.Int.SignExtend(0x801, 0, 11));
            Assert.Equal(-0x002, BitsUtil.Int.SignExtend(0xFFE, 0, 11));
            Assert.Equal(-0x001, BitsUtil.Int.SignExtend(0xFFF, 0, 11));
        }

        [Fact]
        public void LongTests()
        {
            for (var index = 0; index < 64; index++)
            {
                Assert.True(BitsUtil.Long.GetBit(-1, index));
                Assert.False(BitsUtil.Long.GetBit(0, index));

                Assert.Equal(1, BitsUtil.Long.GetBits(-1, index, 1));
                Assert.Equal(0, BitsUtil.Long.GetBits(0, index, 1));
            }

            Assert.Equal(1, BitsUtil.Long.SetBit(0, 0, true));
            Assert.Equal(2, BitsUtil.Long.SetBit(0, 1, true));
            Assert.Equal(4, BitsUtil.Long.SetBit(0, 2, true));
            Assert.Equal(8, BitsUtil.Long.SetBit(0, 3, true));

            Assert.Equal(0x1000000000000000, BitsUtil.Long.SetBit(0, 60, true));
            Assert.Equal(0x2000000000000000, BitsUtil.Long.SetBit(0, 61, true));
            Assert.Equal(0x4000000000000000, BitsUtil.Long.SetBit(0, 62, true));
            Assert.Equal(0x8000000000000000UL, (ulong)BitsUtil.Long.SetBit(0, 63, true));

            Assert.Equal(10 << 2, BitsUtil.Long.SetBits(0, 2, 4, 10));
        }
    }
}
