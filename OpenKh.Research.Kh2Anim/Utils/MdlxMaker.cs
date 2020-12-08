using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenKh.Research.Kh2Anim.Utils
{
    static class MdlxMaker
    {
        internal static MemoryStream CreateMdlxHaving2Bones(float tx = 0)
        {
            var model = Mdlx.CreateModelFromScratch();

            model.SubModels[0].BoneCount = 2;
            model.SubModels[0].Bones.Add(
                new Mdlx.Bone
                {
                    Index = 0,
                    Parent = -1,
                    ScaleX = 1,
                    ScaleY = 1,
                    ScaleZ = 1,
                    ScaleW = 1,
                }
            );
            model.SubModels[0].Bones.Add(
                new Mdlx.Bone
                {
                    Index = 1,
                    Parent = 0,
                    ScaleX = 1,
                    ScaleY = 1,
                    ScaleZ = 1,
                    ScaleW = 1,
                    TranslationX = tx,
                }
            );

            var modelBin = new MemoryStream();
            model.Write(modelBin);

            var mdlxFile = new MemoryStream();

            Bar.Write(
                mdlxFile,
                new Bar.Entry[]
                {
                        new Bar.Entry
                        {
                            Type = Bar.EntryType.Model,
                            Stream = modelBin,
                            Name = "p_ex",
                        },
                }
            );

            return mdlxFile;
        }
    }
}
