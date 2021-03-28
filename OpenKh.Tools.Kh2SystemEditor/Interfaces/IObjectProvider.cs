using OpenKh.Tools.Kh2SystemEditor.Models;
using System.Collections.Generic;

namespace OpenKh.Tools.Kh2SystemEditor.Interfaces
{
    public interface IObjectProvider
    {
        IList<ObjectModel> Objects { get; }

        string GetObjectName(int id);
    }
}
