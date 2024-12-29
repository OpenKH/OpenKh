namespace OpenKh.Patcher.Kh2Ps2Patch.BlockSystem
{
    public static class BlockHelper
    {
        public static int EnsureDescendantOffsets(IBlock block, int top)
        {
            block.EnsuredOffset = top;
            top += block.Length;

            foreach (var one in block.Children)
            {
                top = EnsureDescendantOffsets(one, top);
            }

            return top;
        }
    }
}
