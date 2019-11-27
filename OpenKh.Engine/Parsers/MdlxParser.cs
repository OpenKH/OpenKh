using khkh_xldMii.Mc;
using khkh_xldMii.Mx;
using OpenKh.Engine.Parsers.Kddf2;

namespace OpenKh.Engine.Parsers
{
    public class MdlxParser
    {
        private readonly Mdlxfst _mdlx;
        
        public MdlxParser(Mdlxfst mdlx)
        {
            _mdlx = mdlx;
            Models = System.Array.Empty<Model>();
        }

        public Model[] Models { get; }
    }
}
