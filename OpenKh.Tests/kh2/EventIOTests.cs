using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class EventIOTests
    {
        [Fact]
        public void Read()
        {
            var ardBar = File.OpenRead(@"H:\KH2fm.OpenKH\ard\al00.ard").Using(Bar.Read);
            var eventEntry = ardBar.Single(it => it.Type == Bar.EntryType.Event);
            var eventData = EventIO.Read(eventEntry.Stream);
        }
    }
}
