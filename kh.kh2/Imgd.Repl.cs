namespace kh.kh2
{
    public partial class Imgd
	{
		public static int Repl(int x)
		{
			return (x & 231) | (((x & 16) != 0) ? 8 : 0) | (((x & 8) != 0) ? 16 : 0);
		}
	}
}
