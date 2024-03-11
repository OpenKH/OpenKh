using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Patcher.Kh2Ps2Patch
{
    /// <summary>
    /// Encryptor and decryptor codes are taken from https://github.com/GovanifY/KH2FM_Toolkit
    /// 
    /// KH2FM_Toolkit is programmed by GovanifY https://www.govanify.com https://www.twitter.com/GovanifY
    /// KH2FM_Toolkit Copyright (c) 2015 Gauvain "GovanifY" Roussel-Tarbouriech
    /// </summary>
    public class PatchCodec
    {
        private static readonly byte[] _xTab = Convert.FromBase64String(
            "WAzdWfckf08="
        );
        private static readonly byte[] _gTab = Convert.FromBase64String(
            "pBxrgTANI1tcOqfe2/RzWqDCcNEoSKpyYrWafHwg4MciIHLMJsa8gC14tZXbNyF0BhG1fe+JSNcBp27Qbu58zA=="
        );

        public byte[] ApplyXeeynamosMethod(ReadOnlySpan<byte> input)
        {
            var tab = _xTab;
            var length = input.Length;
            var output = new byte[length];
            for (int index = 0; 0 < length; index++)
            {
                output[index] = (byte)(input[index] ^ tab[--length & 7]);
            }
            return output;
        }

        public byte[] ApplyGovanifYsMethod(ReadOnlySpan<byte> input)
        {
            var tab = _gTab;
            var length = input.Length;
            var output = new byte[length];
            for (int index = 0; 0 < length; index++)
            {
                output[index] = (byte)(input[index] ^ tab[--length & 63]);
            }
            return output;
        }
    }
}
