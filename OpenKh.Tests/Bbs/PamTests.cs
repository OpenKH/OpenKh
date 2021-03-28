using OpenKh.Common;
using OpenKh.Bbs;
using System.IO;
using Xunit;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Tests.Bbs
{
    public class PamTests
    {
        private static readonly string FileName = "Bbs/res/test.pam";

        [Fact]
        public void ReadCorrectData() => File.OpenRead(FileName).Using(stream =>
        {
            var TestPam = Pam.Read(stream);
            Assert.Equal(0x4D4150, (int)TestPam.header.MagicCode);
            Assert.Equal("OpenKHAnim", TestPam.animList[0].AnimEntry.AnimationName);
            Assert.Equal(1.818989362888275E-14, TestPam.animList[0].BoneChannels[1].TranslationX.Header.MaxValue);
            Assert.Equal(-0.027958117425441742, TestPam.animList[0].BoneChannels[2].TranslationX.Header.MaxValue);
            Assert.Equal(0.17539772391319275, TestPam.animList[0].BoneChannels[52].RotationZ.Header.MaxValue);
        });
    }
}
