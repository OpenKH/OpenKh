namespace OpenKh.Engine.Parsers.Kddf2
{
    class TriangleRef
    {
        public TriangleRef(int textureIndex, VertexRef one, VertexRef two, VertexRef three)
        {
            this.textureIndex = textureIndex;
            this.list = new VertexRef[] { one, two, three };
        }

        public VertexRef[] list;
        public int textureIndex;
    }
}
