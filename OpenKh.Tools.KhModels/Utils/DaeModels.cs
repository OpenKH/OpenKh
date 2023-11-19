using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.KhModels.Utils
{
    public static class DaeModels
    {
        public record DaeBone(
            string Name,
            int ParentIndex,
            Vector3 RelativeScale,
            Vector3 RelativeRotation,
            Vector3 RelativeTranslation
        );

        public record DaeTexture(
            string Name,
            string PngFilePath);
    }
}
