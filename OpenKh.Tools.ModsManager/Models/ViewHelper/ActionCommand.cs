using System.Windows.Input;

namespace OpenKh.Tools.ModsManager.Models.ViewHelper
{
    public record ActionCommand(string Display, string ToolTip, ICommand Command)
    {
    }
}
