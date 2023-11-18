using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2FontImageEditor.Helpers
{
    public record FontInfoData(
        byte[]? System,
        byte[]? Event,
        byte[]? Icon)
    {
    }
}
