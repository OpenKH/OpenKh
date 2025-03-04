using OpenKh.Bbs;
using OpenKh.Common;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.CtdEditor.Helpers;
using OpenKh.Tools.CtdEditor.Interfaces;
using OpenKh.Tools.CtdEditor.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.CtdEditor.ViewModels
{
    public class MainViewModel : BaseNotifyPropertyChanged
    {
        private readonly CtdDrawHandler _drawHandler;
        private FontsArc _fonts;

        public MainViewModel(
            CtdDrawHandler drawHandler)
        {
            _drawHandler = drawHandler;
        }

        #region Title
        private string _title = "";
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
        #endregion

        #region FileName
        private string _fileName = "";
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }
        #endregion

        #region FontName
        private string _fontName = "";
        public string FontName
        {
            get => _fontName;
            set
            {
                _fontName = value;
                OnPropertyChanged(nameof(FontName));
            }
        }
        #endregion

        public RelayCommand OpenCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand SaveAsCommand { get; set; }
        public RelayCommand ExitCommand { get; set; }
        public RelayCommand AboutCommand { get; set; }

        public RelayCommand OpenFontCommand { get; set; }
        public RelayCommand OpenFontEditorCommand { get; set; }
        public RelayCommand OpenLayoutEditorCommand { get; set; }

        public RelayCommand UseInternationalEncodingCommand { get; set; }
        public RelayCommand UseJapaneseEncodingCommand { get; set; }

        #region UseInternationalEncoding
        private bool _useInternationalEncoding = true;
        public bool UseInternationalEncoding
        {
            get => _useInternationalEncoding;
            set
            {
                _useInternationalEncoding = value;
                OnPropertyChanged(nameof(UseInternationalEncoding));
            }
        }
        #endregion

        #region UseJapaneseEncoding
        private bool _useJapaneseEncoding = false;
        public bool UseJapaneseEncoding
        {
            get => _useJapaneseEncoding;
            set
            {
                _useJapaneseEncoding = value;
                OnPropertyChanged(nameof(UseJapaneseEncoding));
            }
        }
        #endregion


        #region CtdViewModel
        private CtdViewModel _ctdViewModel = null;
        public CtdViewModel CtdViewModel
        {
            get => _ctdViewModel;
            set
            {
                _ctdViewModel = value;
                OnPropertyChanged(nameof(CtdViewModel));
            }
        }
        #endregion

        public Ctd Ctd
        {
            get => CtdViewModel?.Ctd;
            set
            {
                CtdViewModel = new CtdViewModel(_drawHandler, value, _messageConverter);
                OnPropertyChanged(nameof(OpenLayoutEditorCommand));
            }
        }

        public FontsArc Fonts
        {
            get => _fonts;
            set
            {
                _fonts = value;
                _drawHandler.SetFont(value.FontMes);
                OnPropertyChanged(nameof(OpenFontEditorCommand));
            }
        }

        #region MessageConverter
        private MessageConverter _messageConverter = MessageConverter.Default;
        public MessageConverter MessageConverter
        {
            get => _messageConverter;
            set
            {
                _messageConverter = value;
                OnPropertyChanged(nameof(MessageConverter));

                if (CtdViewModel != null)
                    CtdViewModel.MessageConverter = value;
            }
        }
        #endregion

        public MainViewModel()
        {
            CtdViewModel = new CtdViewModel(_drawHandler, MessageConverter.Default);
        }

    }
}
