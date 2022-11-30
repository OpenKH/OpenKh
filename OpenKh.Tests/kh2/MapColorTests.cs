using Newtonsoft.Json;
using OpenKh.Common;
using OpenKh.Kh2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class MapColorTests
    {
        [Fact]
        public void ReadAndWrite()
        {
            File.OpenRead("kh2/res/eh_1.0e.bin").Using(
                stream =>
                {
                    var model = new MapColorUtil().Read(stream);
                    var json = JsonConvert.SerializeObject(model);
                    Assert.Equal(
                        expected: @"{""BgColor"":{""r"":0,""g"":0,""b"":0,""a"":128},""OnColorTable"":[{""r"":128,""g"":128,""b"":128,""a"":70},{""r"":93,""g"":97,""b"":102,""a"":70},{""r"":79,""g"":84,""b"":90,""a"":70},{""r"":128,""g"":128,""b"":128,""a"":70},{""r"":128,""g"":128,""b"":128,""a"":128},{""r"":128,""g"":128,""b"":128,""a"":128},{""r"":128,""g"":128,""b"":128,""a"":128},{""r"":128,""g"":128,""b"":128,""a"":128},{""r"":128,""g"":128,""b"":128,""a"":128},{""r"":128,""g"":128,""b"":128,""a"":128},{""r"":128,""g"":128,""b"":128,""a"":128},{""r"":128,""g"":128,""b"":128,""a"":128},{""r"":128,""g"":128,""b"":128,""a"":128},{""r"":128,""g"":128,""b"":128,""a"":128},{""r"":128,""g"":128,""b"":128,""a"":128},{""r"":128,""g"":128,""b"":128,""a"":128}],""Fog"":{""FogColor"":{""r"":128,""g"":128,""b"":128,""a"":128},""Min"":1000.0,""Max"":1000000.0,""Near"":0.0,""Far"":255.0}}",
                        actual: json
                    );

                    var reSave = new MemoryStream();
                    new MapColorUtil().Write(reSave, model);

                    var sourceBytes = stream.ReadAllBytes();

                    Assert.Equal(
                        expected: BitConverter.ToString(sourceBytes),
                        actual: BitConverter.ToString(reSave.ToArray())
                    );
                }
            );
        }
    }
}
