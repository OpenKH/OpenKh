using OpenKh.Bbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xe.Tools;
using Xe.Tools.Models;

namespace OpenKh.Tools.CtdEditor.ViewModels
{
    public class LayoutViewModel : BaseNotifyPropertyChanged
    {
        public static EnumModel<Ctd.Arrange> ArrangeTypes { get; } = new EnumModel<Ctd.Arrange>();
        public static EnumModel<Ctd.Style> StyleTypes { get; } = new EnumModel<Ctd.Style>();
        public static EnumModel<Ctd.FontArrange> FontArrangeTypes { get; } = new EnumModel<Ctd.FontArrange>();
        public static EnumModel<Ctd.HookStyle> HookStyleTypes { get; } = new EnumModel<Ctd.HookStyle>();

        private readonly int _id;
        private readonly Ctd.Layout _layout;

        public int Id => _id;
        
        public ushort DialogX
        {
            get => _layout.DialogX;
            set => _layout.DialogX = value;
        }

        public ushort DialogY
        {
            get => _layout.DialogY;
            set => _layout.DialogY = value;
        }

        public ushort DialogWidth
        {
            get => _layout.DialogWidth;
            set => _layout.DialogWidth = value;
        }

        public ushort DialogHeight
        {
            get => _layout.DialogHeight;
            set => _layout.DialogHeight = value;
        }

        public Ctd.Arrange DialogAlignment
        {
            get => _layout.DialogAlignment;
            set => _layout.DialogAlignment = value;
        }
        
        public Ctd.Style DialogBorders
        {
            get => _layout.DialogBorders;
            set => _layout.DialogBorders = value;
        }
        
        public ushort TextAlignment
        {
            get => (ushort)_layout.TextAlignment;
            set => _layout.TextAlignment = (Ctd.FontArrange)value;
        }

        public ushort FontSize
        {
            get => _layout.FontSize;
            set => _layout.FontSize = value;
        }

        public ushort HorizontalSpace
        {
            get => _layout.HorizontalSpace;
            set => _layout.HorizontalSpace = value;
        }

        public ushort VerticalSpace
        {
            get => _layout.VerticalSpace;
            set => _layout.VerticalSpace = value;
        }

        public ushort TextX
        {
            get => _layout.TextX;
            set => _layout.TextX = value;
        }

        public ushort TextY
        {
            get => _layout.TextY;
            set => _layout.TextY = value;
        }

        public Ctd.HookStyle DialogHook
        {
            get => _layout.DialogHook;
            set => _layout.DialogHook = value;
        }

        public ushort DialogHookX
        {
            get => _layout.DialogHookX;
            set => _layout.DialogHookX = value;
        }

        public ushort TextColorIdx
        {
            get => _layout.TextColorIdx;
            set => _layout.TextColorIdx = value;
        }


        public LayoutViewModel(int id, Ctd.Layout layout)
        {
            _id = id;
            _layout = layout;
        }
    }

    public class LayoutEditorViewModel : BaseNotifyPropertyChanged
    {
        private readonly Ctd _ctd;
        private readonly List<LayoutViewModel> _layouts;
        private LayoutViewModel _selectedLayout;

        public LayoutEditorViewModel(Ctd ctd)
        {
            _ctd = ctd;
            _layouts = ctd.Layouts.Select((l, i) => new LayoutViewModel(i, l)).ToList();
        }

        public List<LayoutViewModel> Layouts => _layouts;

        public LayoutViewModel SelectedLayout
        {
            get => _selectedLayout;
            set
            {
                _selectedLayout = value;
                OnPropertyChanged(nameof(SelectedLayout));
                OnPropertyChanged(nameof(IsLayoutSelected));
            }
        }

        public bool IsLayoutSelected => _selectedLayout != null;
    }
}
