using OpenKh.Engine.Maths;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Engine.Parsers.Kddf2.Mset.Interfaces
{
    public interface IAnimMatricesProvider
    {
        Matrix[] ProvideMatrices(double delta);
    }
}
