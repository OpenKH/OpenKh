using OpenKh.Engine.Maths;
using OpenKh.Engine.Parsers.Kddf2.Mset.EmuRunner;
using OpenKh.Engine.Parsers.Kddf2.Mset.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenKh.Engine.Parsers.Kddf2.Mset
{
    public class EmuBasedAnimMatricesProvider : IAnimMatricesProvider
    {
        private readonly Mlink emuRunner;
        private readonly AnimReader animReader;
        private readonly uint anbAbsOff;
        private double absTime = 0;

        public EmuBasedAnimMatricesProvider(
            AnimReader animReader,
            uint anbAbsOff,
            Stream mdlxStream,
            Stream msetStream
        )
        {
            emuRunner = new Mlink();
            this.animReader = animReader;
            this.anbAbsOff = anbAbsOff;

            var matrixOutStream = new MemoryStream();

            mdlxStream.Position = 0;
            msetStream.Position = 0;

            // initialize emulator memory space
            emuRunner.Permit(
                mdlxStream, animReader.cntb1,
                msetStream, animReader.cntb2,
                anbAbsOff, (float)absTime, matrixOutStream
            );
        }

        public Matrix[] ProvideMatrices(double delta)
        {
            absTime += delta;

            var matrixOutStream = new MemoryStream();

            emuRunner.Permit(
                null, animReader.cntb1,
                null, animReader.cntb2,
                anbAbsOff, (float)absTime, matrixOutStream
            );

            BinaryReader br = new BinaryReader(matrixOutStream);
            matrixOutStream.Position = 0;
            var matrixOut = new Matrix[animReader.cntb1];
            for (int t = 0; t < animReader.cntb1; t++)
            {
                Matrix M1 = new Matrix();
                M1.M11 = br.ReadSingle(); M1.M12 = br.ReadSingle(); M1.M13 = br.ReadSingle(); M1.M14 = br.ReadSingle();
                M1.M21 = br.ReadSingle(); M1.M22 = br.ReadSingle(); M1.M23 = br.ReadSingle(); M1.M24 = br.ReadSingle();
                M1.M31 = br.ReadSingle(); M1.M32 = br.ReadSingle(); M1.M33 = br.ReadSingle(); M1.M34 = br.ReadSingle();
                M1.M41 = br.ReadSingle(); M1.M42 = br.ReadSingle(); M1.M43 = br.ReadSingle(); M1.M44 = br.ReadSingle();
                matrixOut[t] = M1;
            }

            return matrixOut;
        }
    }
}
