namespace OpenKh.Engine.Parsers.Kddf2
{
    public class MJ1
    {
        public int matrixIndex;
        public int vertexIndex;
        public float factor;

        public MJ1(int matrixIndex, int vertexIndex, float factor)
        {
            this.matrixIndex = matrixIndex;
            this.vertexIndex = vertexIndex;
            this.factor = factor;
        }
    }
}
