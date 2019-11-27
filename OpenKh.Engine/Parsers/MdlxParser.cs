using OpenKh.Kh2;

namespace OpenKh.Engine.Parsers
{
    public class MdlxParser
    {
        private readonly Mdlx _mdlx;
        
        public MdlxParser(Mdlx mdlx)
        {
            _mdlx = mdlx;
            Models = System.Array.Empty<Model>();
        }

        public Model[] Models { get; }
    }
}
