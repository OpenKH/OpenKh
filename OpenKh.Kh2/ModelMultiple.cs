using System;
using System.IO;

namespace OpenKh.Kh2
{
    public class ModelMultiple : Model
    {
        private bool _isParsedSuccessfully;
        private string _errorMessage;

        public ModelMultiple(Stream stream)
        {
            try
            {
                //Parse Logic, doesn't work for now but fixes the crash for tr02/etc.
                _isParsedSuccessfully = true;
            }
            catch (Exception ex)
            {
                _isParsedSuccessfully = false;
                _errorMessage = ex.Message;
            }
        }


        public override int GroupCount => 0;

        protected override void InternalWrite(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
