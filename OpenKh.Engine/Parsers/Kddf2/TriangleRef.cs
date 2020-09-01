namespace OpenKh.Engine.Parsers.Kddf2
{
    class TriangleRef
    {
        public TriangleRef(int textureIndex, bool isOpaque, VertexRef one, VertexRef two, VertexRef three)
        {
            this.textureIndex = textureIndex;
            this.isOpaque = isOpaque;
            this.list = new VertexRef[] { one, two, three };
        }

        public VertexRef[] list;
        public int textureIndex;
        public bool isOpaque;
    }
}
