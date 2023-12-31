using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenKh.Tools.ModsManager.Models.ViewHelper
{
    public record ActionCommand(string Display, ICommand Command)
    {
    }
}
