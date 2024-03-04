using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Xe.Tools;

namespace OpenKh.Tools.Kh2FontImageEditor.Views
{
    public class SpacingWindowVM : BaseNotifyPropertyChanged
    {
        public record StateModel(
            ImageSource Image,
            int Width,
            int Height,
            ICommand SaveCommand,
            Action<int, int, int> AdjustSpacing
        );

        #region State property
        private StateModel? _state;
        public StateModel? State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

    }
}
