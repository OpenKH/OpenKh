using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenKh.Kh2Anim.Mset.Interfaces
{
    public interface IAnimMatricesProvider
    {
        Matrix4x4[] ProvideMatrices(double gameTimeDelta);

        float FrameLoop { get; }
        float FrameEnd { get; }
        float FramePerSecond { get; }
        float FrameCount { get; }
        int MatrixCount { get; }
    }
}
