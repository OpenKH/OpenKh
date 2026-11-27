using System;

namespace OpenKh.Patcher.Kh2Ps2Patch
{
    /// <summary>
    /// 
    /// </summary>
    public class PatchCodec
    {
        /// Attributions:
        /// 
        /// The original KH2PATCH format (Noted here as "Legacy") has been devised by Xeeynamo.
        /// The new KH2PATCH format (Noted here as "New") is the original format, improved upon by CrazyCatz00.
        /// 
        /// Signature for the legacy format is 0x5032484B, for the new format is 0x5132484B.
        /// 
        /// The new format can be compatible with the legacy format as long as it adheres to these differences:
        /// 
        /// - Legacy format cannot edit OVL.IDX, it also can't edit the ISO direct.
        /// - Legacy format is unable to add non-existing files.
        /// - Legacy format patches cannot be decrypted.
        
        private static readonly byte[] _xTab = Convert.FromBase64String(
            "WAzdWfckf08="
        );
        private static readonly byte[] _cTab = Convert.FromBase64String(
            "pBxrgTANI1tcOqfe2/RzWqDCcNEoSKpyYrWafHwg4MciIHLMJsa8gC14tZXbNyF0BhG1fe+JSNcBp27Qbu58zA=="
        );

        public byte[] ApplyLegacyFormatMethod(ReadOnlySpan<byte> input)
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

        public byte[] ApplyNewFormatMethod(ReadOnlySpan<byte> input)
        {
            var tab = _cTab;
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
