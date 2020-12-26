using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using OpenKh.Tools.Common.Rendering;
using OpenKh.Tools.Kh2TextEditor.Interfaces;
using OpenKh.Tools.Kh2TextEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;

using ColorPickerWPF;
using System.Windows.Media;

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
        private RenderingMessageContext textContext;
        private bool _showErrors;

        public RelayCommand HandleReturn { get; }
        public RelayCommand HandleAddition { get; }
        public RelayCommand HandleRemoval { get; }
        public RelayCommand HandleComm { get; }
        public RelayCommand HandleIcon { get; }

        public List<Msg.Entry> MessageEntries
        {
            get => _msgs;
            set
            {
                _msgs = value;
                ResetMessagesView();
            }
        }

        public ISpriteDrawing Drawing { get; }

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
                OnPropertyChanged();
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

        public RenderingMessageContext TextContext
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
            Drawing = new SpriteDrawingDirect3D();
            CurrentMessageEncoder = Encoders.InternationalSystem;
            _messages = new MessagesModel(this, this, new Msg.Entry[] { });

            HandleReturn = new RelayCommand(x =>
            {
                Text += "{:newline}";                
            });

            HandleRemoval = new RelayCommand(x =>
            {
                MessageEntries.RemoveAll(x => x.Id == SelectedItem.Id);
                ResetMessagesView();
            });

            HandleAddition = new RelayCommand(x =>
            {
                int _id = MessageEntries.Max(x => x.Id);
                MessageEntries.Add(new Msg.Entry() { Id = _id + 1, Data = CurrentMessageEncoder.Encode(MsgSerializer.DeserializeText("FAKE").ToList()) });
                ResetMessagesView();
            });

            HandleComm = new RelayCommand(x =>
            {
                int _o = Convert.ToInt32(x);

                switch (_o)
                {
                    case 0:
                        Text += "{:reset}";
                        break;
                    case 1:
                    {
                        Color _color;
                        bool _ok = ColorPickerWindow.ShowDialog(out _color);

                        if (_ok)
                            Text += "{:color " + string.Format("#{0}{1}{2}{3}", _color.R.ToString("X2"), _color.G.ToString("X2"), _color.B.ToString("X2"), _color.A.ToString("X2")) + "}";
                    }
                        break;
                    case 2:
                        Text += "{:scale 16}";
                        break;
                    case 3:
                        Text += "{:width 100}";
                        break;
                    case 4:
                        Text += "{:clear}";
                        break;
                    case 5:
                        Text += "{:position 0,0}";
                        break;
                }
            });

            HandleIcon = new RelayCommand(x =>
            {
                Text += "{:icon " + x + "}";
            });
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
            arg.Title.ToLower().Contains(SearchTerm.ToLower());
    }
}
