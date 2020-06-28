using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Kh2.Mixdata
{
    public class Exp
    {
        private const int MagicCode = 0x5845494D;

        public List<Exp> Read(Stream stream) => BaseMixdata<Exp>.Read(stream).Items;
        public void Write(Stream stream, int version, IEnumerable<Exp> items) => BaseMixdata<Exp>.Write(stream, MagicCode, version, items.ToList());
    }
}
