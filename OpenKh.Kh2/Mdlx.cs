using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.IO;

namespace OpenKh.Kh2
{
    public partial class Mdlx
    {
        private const int Map = 2;
        private const int Entity = 3;
        private const int Shadow = 4;
        private const int ReservedArea = 0x90;

        public List<SubModel> SubModels { get; }
        public Model Model { get; }
        public ModelBackground ModelBackground => Model as ModelBackground;

        private Mdlx(Stream stream)
        {
            var type = ReadMdlxType(stream);
            stream.FromBegin();

            switch (type)
            {
                case Entity:
                    SubModels = ReadAsModel(stream).ToList();
                    break;
                default:
                    Model = Model.Read(stream.FromBegin());
                    break;
            }
        }

        public bool IsMap => Model is ModelBackground;

        public void Write(Stream stream)
        {
            if (Model != null)
                Model.Write(stream);
            else
            {
                var tempStream = new MemoryStream();
                WriteAsModel(tempStream, SubModels);
                stream.Position = ReservedArea;
                stream.Write(tempStream.GetBuffer(), 0, (int)tempStream.Length);
            }
        }

        private static void WriteAsModel(Stream stream, List<SubModel> subModels)
        {
            var baseAddress = 0;
            for (var i = 0; i < subModels.Count; i++)
            {
                if (i + 1 >= subModels.Count)
                    baseAddress = -1;

                var subModelStream = new MemoryStream();
                WriteSubModel(subModelStream, subModels[i], baseAddress);
                subModelStream.SetPosition(0).Copy(stream, (int)subModelStream.Length);
            }
        }

        public static Mdlx Read(Stream stream) =>
            new Mdlx(stream.SetPosition(0));

        private static int ReadMdlxType(Stream stream) =>
            stream.SetPosition(ReservedArea).ReadInt32();

        private static T[] For<T>(int count, Func<T> func) =>
            Enumerable.Range(0, count).Select(_ => func()).ToArray();

        private Mdlx(Model model)
        {
            Model = model;
        }

        public static Mdlx CreateFromMapModel(ModelBackground model) => new(model);

        private Mdlx()
        {
            SubModels = new List<SubModel>
            {
                new SubModel
                {
                    Type = Entity,
                    Bones = new List<Bone>(),
                    DmaChains = new List<DmaChain>(),
                },
                new SubModel
                {
                    Type = Shadow,
                    Bones = new List<Bone>(),
                    DmaChains = new List<DmaChain>(),
                }
            };
        }

        public static Mdlx CreateModelFromScratch() => new Mdlx();
    }
}
