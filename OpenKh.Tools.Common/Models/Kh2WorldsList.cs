using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xe.Tools.Models;
using OpenKh.Kh2;

namespace OpenKh.Tools.Common.Models
{
    public class Kh2WorldsList : IEnumerable<EnumItemModel<World>>, IEnumerable
    {
        private static List<EnumItemModel<World>> _list = Enum.GetValues(typeof(World))
                .Cast<World>()
                .Select(e => new EnumItemModel<World>()
                {
                    Value = e,
                    Name = Constants.WorldNames[(int)e]
                })
                .ToList();

        public IEnumerator<EnumItemModel<World>> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }
}
