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
                    SubModels = new Mdlxfst(stream).SubModels;
                    break;
            }
        }

        public static Mdlx Read(Stream stream) =>
            new Mdlx(stream.SetPosition(0));

        private static int ReadMdlxType(Stream stream) =>
            stream.SetPosition(ReservedArea).ReadInt32();

        private static T[] For<T>(int count, Func<T> func) =>
            Enumerable.Range(0, count).Select(_ => func()).ToArray();
    }
}
