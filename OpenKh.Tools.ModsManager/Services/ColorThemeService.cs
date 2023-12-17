using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xe.Tools;

namespace OpenKh.Tools.ModsManager.Services
{
    public class ColorThemeService : BaseNotifyPropertyChanged
    {
        public bool DarkMode
        {
            get
            {
                return ConfigurationService.DarkMode;
            }
            set
            {
                ConfigurationService.DarkMode = value;

                OnPropertyChanged(nameof(DarkMode));
                OnPropertyChanged(nameof(BackgroundColor));
                OnPropertyChanged(nameof(TextColor));
            }
        }

        public string BackgroundColor
        {
            get
            {
                if (DarkMode)
                {
                    return "#2D2D2D";
                }
                else
                {
                    return "white";
                }
            }
        }

        public string TextColor
        {
            get
            {
                if (ConfigurationService.DarkMode)
                {
                    return "white";
                }
                else
                {
                    return "black";
                }
            }
        }

        public static readonly ColorThemeService Instance = new ColorThemeService();
    }
}
