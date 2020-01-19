using OpenKh.Kh2;
using OpenKh.Kh2.Contextes;
using OpenKh.Kh2.Messages;
using OpenKh.Tools.Common.Models;
using OpenKh.Tools.Kh2TextEditor.Interfaces;
using OpenKh.Tools.Kh2TextEditor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Xe.Drawing;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;

namespace OpenKh.Tools.Kh2TextEditor.ViewModels
{
    public class TextEditorViewModel : BaseNotifyPropertyChanged, ICurrentMessageEncoder, IInvalidateErrorCount
    {
        private MessagesModel _messages;
        private List<Msg.Entry> _msgs;
        private IMessageEncoder _currentMessageEncoder;
        private MessageModel _selectedItem;
        private string _currentText;
        private string _searchTerm;
        private KingdomTextContext textContext;
        private bool _showErrors;

        public List<Msg.Entry> MessageEntries
        {
            get => _msgs;
            set
            {
                _msgs = value;
                ResetMessagesView();
            }
        }

        public IDrawing Drawing { get; }

        public MessagesModel Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                OnPropertyChanged();
            }
        }

        public MessageModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                _currentText = _selectedItem?.Text;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Text));
            }
        }

        public string Text
        {
            get => _currentText;
            set
            {
                var selectedItem = SelectedItem;
                if (selectedItem == null)
                    return;

                _currentText = value;
                selectedItem.Text = value;
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                PerformFiltering();
            }
        }

        public KingdomTextContext TextContext
        {
            get => textContext;
            set
            {
                textContext = value;
                OnPropertyChanged();

                if (CurrentMessageEncoder != textContext.Encoder)
                {
                    CurrentMessageEncoder = textContext.Encoder;
                }
            }
        }

        public IMessageEncoder CurrentMessageEncoder
        {
            get => _currentMessageEncoder;
            private set
            {
                _currentMessageEncoder = value;
                _messages?.InvalidateText();
                OnPropertyChanged(nameof(SelectedItem));
                OnPropertyChanged();
            }
        }

        public bool ShowErrors
        {
            get => _showErrors;
            set
            {
                _showErrors = value;
                PerformFiltering();
            }
        }

        public int ErrorCount => _messages.Items.Count(x => !x.DoesNotContainErrors);

        public Visibility AnyErrorVisibility => ErrorCount == 0 ? Visibility.Collapsed : Visibility.Visible;

        public TextEditorViewModel()
        {
            Drawing = new DrawingDirect3D();
            CurrentMessageEncoder = Encoders.InternationalSystem;
            _messages = new MessagesModel(this, this, new Msg.Entry[] { });
        }

        public void SelectMessage(int id) => SelectedItem = Messages.GetMessage(id);

        public void InvalidateErrorCount()
        {
            if (!_messages.Items.Any(x => !x.DoesNotContainErrors) && ShowErrors)
                ShowErrors = false;

            OnPropertyChanged(nameof(ErrorCount));
            OnPropertyChanged(nameof(AnyErrorVisibility));
        }

        private void ResetMessagesView()
        {
            if (MessageEntries != null)
                Messages = new MessagesModel(this, this, MessageEntries);
        }

        private void PerformFiltering()
        {
            if (string.IsNullOrWhiteSpace(_searchTerm))
                _messages.Filter(x => Filter(x, FilterNone));
            else
                _messages.Filter(x => Filter(x, FilterTextAndId));
        }

        private bool Filter(MessageModel arg, Func<MessageModel, bool> filter) =>
            FilterError(arg) && filter(arg);

        private bool FilterError(MessageModel arg) => !ShowErrors || (ShowErrors && arg.DoesNotContainErrors == false);

        private bool FilterNone(MessageModel arg) => true;

        private bool FilterTextAndId(MessageModel arg) =>
            $"{arg.Id.ToString()} {arg.Text}".ToLower().Contains(SearchTerm.ToLower());
    }
}
