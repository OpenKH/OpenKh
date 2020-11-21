using OpenKh.Kh2Anim.Mset.EmuRunner;
using OpenKh.Kh2Anim.Mset.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace OpenKh.Kh2Anim.Mset
{
    public class EmuBasedAnimMatricesProvider : IAnimMatricesProvider
    {
        private readonly Mlink emuRunner;
        private readonly AnimReader animReader;
        private double absTime = 0;

        public EmuBasedAnimMatricesProvider(
            AnimReader animReader,
            Stream mdlxStream,
            Stream animStream
        )
        {
            emuRunner = new Mlink();
            this.animReader = animReader;

            var matrixOutStream = new MemoryStream();

            mdlxStream.Position = 0;
            animStream.Position = 0;

            // initialize emulator memory space
            emuRunner.Permit(
                mdlxStream, animReader.cntb1,
                animStream, animReader.cntb2,
                0, (float)absTime, matrixOutStream
            );
        }

        float IAnimMatricesProvider.FrameLoop => animReader.FrameLoop;

        float IAnimMatricesProvider.FrameEnd => animReader.FrameEnd;

        float IAnimMatricesProvider.FramePerSecond => animReader.FramePerSecond;

        float IAnimMatricesProvider.FrameCount => animReader.FrameCount;

        int IAnimMatricesProvider.MatrixCount => animReader.cntb2;

        Matrix4x4[] IAnimMatricesProvider.ProvideMatrices(double delta)
        {
            absTime += delta;

            var matrixOutStream = new MemoryStream();

            emuRunner.Permit(
                null, animReader.cntb1,
                null, animReader.cntb2,
                0, (float)absTime, matrixOutStream
            );

            var br = new BinaryReader(matrixOutStream);
            matrixOutStream.Position = 0;
            var matrixOut = new Matrix4x4[animReader.cntb1];
            for (int t = 0; t < animReader.cntb1; t++)
            {
                var m = new Matrix4x4();
                m.M11 = br.ReadSingle();
                m.M12 = br.ReadSingle();
                m.M13 = br.ReadSingle();
                m.M14 = br.ReadSingle();
                m.M21 = br.ReadSingle();
                m.M22 = br.ReadSingle();
                m.M23 = br.ReadSingle();
                m.M24 = br.ReadSingle();
                m.M31 = br.ReadSingle();
                m.M32 = br.ReadSingle();
                m.M33 = br.ReadSingle();
                m.M34 = br.ReadSingle();
                m.M41 = br.ReadSingle();
                m.M42 = br.ReadSingle();
                m.M43 = br.ReadSingle();
                m.M44 = br.ReadSingle();
                matrixOut[t] = m;
            }

            return matrixOut;
        }
    }
}
