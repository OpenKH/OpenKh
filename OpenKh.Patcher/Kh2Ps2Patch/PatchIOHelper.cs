using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Patcher.Kh2Ps2Patch
{
    public class PatchIOHelper
    {
        private readonly PatchIO _patchIo = new PatchIO();
        private readonly PatchCodec _patchCodec = new PatchCodec();

        public PatchHeader Read(Memory<byte> memory)
        {
            if (_patchIo.VerifySignature(memory))
            {
                return _patchIo.Read(memory);
            }

            {
                var decoded = _patchCodec.ApplyXeeynamosMethod(memory.Span);
                if (_patchIo.VerifySignature(decoded))
                {
                    return _patchIo.Read(decoded);
                }
            }

            {
                var decoded = _patchCodec.ApplyGovanifYsMethod(memory.Span);
                if (_patchIo.VerifySignature(decoded))
                {
                    return _patchIo.Read(decoded);
                }
            }

            throw new InvalidDataException("Invalid patch file");
        }
    }
}
