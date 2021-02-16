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
        public M4 MapModel { get; }

        private Mdlx(Stream stream)
        {
            var type = ReadMdlxType(stream);
            stream.Position = 0;

            switch (type)
            {
                case Map:
                    MapModel = ReadAsMap(new SubStream(stream, ReservedArea, stream.Length - ReservedArea));
                    break;
                case Entity:
                    SubModels = ReadAsModel(stream).ToList();
                    break;
            }
        }

        public bool IsMap => MapModel != null;

        public void Write(Stream realStream)
        {
            var stream = new MemoryStream();
            if (IsMap)
                WriteAsMap(stream, MapModel);
            else
                WriteAsModel(stream, SubModels);

            realStream.Position = ReservedArea;
            realStream.Write(stream.GetBuffer(), 0, (int)stream.Length);
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

        private Mdlx(M4 mapModel)
        {
            this.MapModel = mapModel;
        }

        public static Mdlx CreateFromMapModel(M4 mapModel) => new Mdlx(mapModel);

        private Mdlx()
        {
            SubModels = new List<SubModel>();
            SubModels.Add(new SubModel
            {
                Type = Entity,
                Bones = new List<Bone>(),
                DmaChains = new List<DmaChain>(),
            });
            SubModels.Add(new SubModel
            {
                Type = Shadow,
                Bones = new List<Bone>(),
                DmaChains = new List<DmaChain>(),
            });
        }

        public static Mdlx CreateModelFromScratch() => new Mdlx();
    }
}
