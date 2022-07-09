using System;
using System.IO;

namespace OpenKh.Kh2
{
    public class ModelMultiple : Model
    {
        public ModelMultiple(Stream stream)
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
