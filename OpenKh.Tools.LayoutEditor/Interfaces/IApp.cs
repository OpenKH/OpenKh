using System;

namespace OpenKh.Tools.LayoutEditor.Interfaces
{
    public interface IApp : IDisposable
    {
        void Menu();
        bool Run();
    }
}
