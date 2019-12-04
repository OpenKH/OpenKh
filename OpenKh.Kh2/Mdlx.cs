using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Kh2
{
    public partial class Mdlx
    {
        private const int Map = 2;
        private const int Entity = 3;

        public List<SubModel> SubModels { get; }

        private Mdlx(Stream stream)
        {
            var type = ReadMdlxType(stream);
            stream.Position = 0;

            switch (type)
            {
                case Map:
                    throw new NotImplementedException($"The MDLX {nameof(Map)} type is not implemented");
                case Entity:
                    SubModels = new Mdlxfst(stream).SubModels;
                    break;
            }
        }

        public static Mdlx Read(Stream stream) =>
            new Mdlx(stream.SetPosition(0));

        private static int ReadMdlxType(Stream stream) =>
            stream.SetPosition(0x90).ReadInt32();

        private static IEnumerable<T> For<T>(int count, Func<T> func) =>
            Enumerable.Range(0, count).Select(_ => func());
    }
}
