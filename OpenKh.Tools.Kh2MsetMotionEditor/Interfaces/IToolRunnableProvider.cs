using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Interfaces
{
    public interface IToolRunnableProvider
    {
        Action CreateToolRunnable();
    }
}
