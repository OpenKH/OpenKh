using System;
using System.IO;

namespace OpenKh.Kh2
{
    public class ModelLegacy : Model
    {
        public ModelLegacy(Stream stream)
        {
            throw new NotImplementedException();
        }

        public override int GroupCount => 0;

        protected override void InternalWrite(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
