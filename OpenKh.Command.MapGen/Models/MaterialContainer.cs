using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.MapGen.Models
{
    public class MaterialContainer
    {
        public List<MaterialDef> Materials { get; } = new List<MaterialDef>();

        internal short Add(MaterialDef matDef)
        {
            int hit = Materials.FindIndex(it => it == matDef);
            if (hit == -1)
            {
                var textureIndex = (short)-1;
                if (!matDef.nodraw)
                {
                    textureIndex = (short)Materials.Count;
                    Materials.Add(matDef);
                }

                return textureIndex;
            }
            else
            {
                return Convert.ToInt16(hit);
            }
        }
    }
}
