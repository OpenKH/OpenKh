namespace OpenKh.Engine.Parsers.Kddf2
{
    class Ff3
    {
        public Ff3(int texi, Ff1 x, Ff1 y, Ff1 z)
        {
            this.texi = texi;
            this.al1 = new Ff1[] { x, y, z };
        }

        public Ff1[] al1;
        public int texi;
    }
}
