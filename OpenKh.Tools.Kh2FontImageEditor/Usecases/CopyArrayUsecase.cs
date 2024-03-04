using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2FontImageEditor.Usecases
{
    public class CopyArrayUsecase
    {
        internal byte[] Copy(byte[] array)
        {
            var newArray = new byte[array.Length];
            Buffer.BlockCopy(array, 0, newArray, 0, array.Length);
            return newArray;
        }
    }
}
