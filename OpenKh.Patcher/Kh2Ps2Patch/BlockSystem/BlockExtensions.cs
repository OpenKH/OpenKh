using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Patcher.Kh2Ps2Patch.BlockSystem
{
    internal static class BlockExtensions
    {
        public static T PrependTo<T>(this T self, IBlock parent) where T : IBlock
        {
            parent.Children.Insert(0, self);
            return self;
        }

        public static T AppendTo<T>(this T self, IBlock parent) where T : IBlock
        {
            parent.Children.Add(self);
            return self;
        }

        public static Memory<byte> SliceBuffer(this IBlock self, Memory<byte> buffer)
        {
            return buffer.Slice(self.EnsuredOffset, self.Length);
        }

        public static IBlock Tag(this IBlock self, string tag) => self.Children.Single(it => it.Tag == tag);

        public static IEnumerable<IBlock> Tags(this IBlock self, string tag) => self.Children.Where(it => it.Tag == tag);
    }
}
