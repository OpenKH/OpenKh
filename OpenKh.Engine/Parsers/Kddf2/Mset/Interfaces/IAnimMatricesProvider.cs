using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenKh.Engine.Parsers.Kddf2.Mset.Interfaces
{
    public interface IAnimMatricesProvider
    {
        Matrix4x4[] ProvideMatrices(double gameTimeDelta);
    }
}
