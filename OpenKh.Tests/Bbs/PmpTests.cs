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
        private static readonly string FileName = "Bbs/res/jb_50.pmp";

        [Fact]
        public void ReadCorrectHeader() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPmo = Pmp.Read(stream);
            Assert.Equal(0x504D50, (int)TestPmo.header.MagicCode);
            Assert.Equal(4, (int)TestPmo.header.ObjectCount);
        });
    }
}
