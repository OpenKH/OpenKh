using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Interfaces
{
    public interface IWindowRunnableProvider
    {
        Action CreateWindowRunnable();
    }
}