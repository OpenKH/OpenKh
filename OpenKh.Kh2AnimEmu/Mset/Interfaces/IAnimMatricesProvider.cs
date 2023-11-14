using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenKh.Kh2Anim.Mset.Interfaces
{
    public interface IAnimMatricesProvider
    {
        void ResetGameTimeDelta();
        Matrix4x4[] ProvideMatrices(double gameTimeDelta);
        (Matrix4x4[] Fk, Matrix4x4[] Ik) ProvideMatrices2(double gameTimeDelta);

        float FrameLoop { get; }
        float FrameEnd { get; }
        float FramePerSecond { get; }
        float FrameCount { get; }
        int MatrixCount { get; }
    }
}
