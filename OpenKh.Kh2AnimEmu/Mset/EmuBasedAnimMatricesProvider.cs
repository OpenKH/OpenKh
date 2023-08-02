using OpenKh.Common;
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

        public void ResetGameTimeDelta()
        {
            absTime = 0;
        }

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
                matrixOut[t] = matrixOutStream.ReadMatrix4x4();

            return matrixOut;
        }

        public (Matrix4x4[] Fk, Matrix4x4[] Ik) ProvideMatrices2(double gameTimeDelta)
        {
            absTime += gameTimeDelta;

            var matrixOutStream = new MemoryStream();

            emuRunner.Permit(
                null, animReader.cntb1,
                null, animReader.cntb2,
                0, (float)absTime, matrixOutStream
            );

            matrixOutStream.Position = 0;

            var fk = new Matrix4x4[animReader.cntb1];
            for (int t = 0; t < animReader.cntb1; t++)
            {
                fk[t] = matrixOutStream.ReadMatrix4x4();
            }

            var numIk = animReader.cntb2 - animReader.cntb1;
            var ik = new Matrix4x4[numIk];
            for (int t = 0; t < numIk; t++)
            {
                ik[t] = matrixOutStream.ReadMatrix4x4();
            }

            return (fk, ik);
        }

    }
}
