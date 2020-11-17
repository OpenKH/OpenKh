namespace OpenKh.Engine.Parsers.Kddf2
{
    class TriangleRef
    {
        public TriangleRef(VertexRef one, VertexRef two, VertexRef three)
        {
            this.list = new VertexRef[] { one, two, three };
        }

        public VertexRef[] list;
    }
}
