namespace OpenKh.Engine.Parsers.Kddf2
{
    public class JointAssignment
    {
        public int matrixIndex;
        public int vertexIndex;
        public float factor;

        public JointAssignment(int matrixIndex, int vertexIndex, float factor)
        {
            this.matrixIndex = matrixIndex;
            this.vertexIndex = vertexIndex;
            this.factor = factor;
        }
    }
}
