using OpenKh.Common;
using OpenKh.Bbs;
using System.IO;
using Xunit;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Tests.Bbs
{
    public class PmpTests
    {
        private static readonly string FileName = "Bbs/res/jb_23.pmp";

        [Fact]
        public void ReadCorrectHeader() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmp.Read(stream);
            Assert.Equal(0x504D50, (int)TestPmo.header.MagicCode);
            Assert.Equal(4, (int)TestPmo.header.ObjectCount);
        });

        [Fact]
        public void WritesBackCorrectly()
        {
            Stream input = File.OpenRead(FileName);
            var TestPmp = Pmp.Read(input);
            Stream output = File.Open("Bbs/res/jb_23_TEST.pmp", FileMode.Create);
            Pmp.Write(output, TestPmp);

            input.Position = 0;
            output.Position = 0;

            // Check all bytes.
            for (int i = 0; i < output.Length; i++)
            {
                if (input.ReadByte() != output.ReadByte())
                {
                    long position = output.Position;
                    Assert.False(true);
                }
            }

            Assert.True(true);
        }
    }
}
