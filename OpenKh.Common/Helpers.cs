namespace OpenKh.Common
{
    public class Helpers
    {
        public static int Align(int offset, int alignment)
        {
            var misalignment = offset % alignment;
            return misalignment > 0 ? offset + alignment - misalignment : offset;
        }
    }
}
