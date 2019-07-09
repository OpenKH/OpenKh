using OpenKh.Kh2;
using OpenKh.Kh2.Contextes;
using OpenKh.Kh2.Messages;
using OpenKh.Tools.Common.Models;
using OpenKh.Tools.Kh2TextEditor.Interfaces;
using OpenKh.Tools.Kh2TextEditor.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.Drawing;
using Xe.Tools;

namespace OpenKh.Tools.Kh2TextEditor.ViewModels
{
    public class TextEditorViewModel : BaseNotifyPropertyChanged, ICurrentMessageEncoder
    {
        private MessagesModel _messages;
        private List<Msg.Entry> _msgs;
        private IMessageEncoder _currentMessageEncoder;
        private MessageModel _selectedItem;
        private string _currentText;
        private string _searchTerm;
        private KingdomTextContext textContext;

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

                if (string.IsNullOrWhiteSpace(_searchTerm))
                    _messages.Filter(FilterNone);
                else
                    _messages.Filter(FilterTextAndId);
            }
        }

        public KingdomTextContext TextContext
        {
            get => textContext;
            set
            {
                textContext = value;
                OnPropertyChanged();
            }
        }

        public IMessageEncoder CurrentMessageEncoder
        {
            get => _currentMessageEncoder;
            private set
            {
                _currentMessageEncoder = value;
                OnPropertyChanged();
            }
        }

        public TextEditorViewModel()
        {
            Drawing = new DrawingDirect3D();
            CurrentMessageEncoder = Encoders.InternationalSystem;
            _messages = new MessagesModel(this, new Msg.Entry[] { });
        }

        public void SelectMessage(int id) => SelectedItem = Messages.GetMessage(id);

        private void ResetMessagesView()
        {
            Messages = new MessagesModel(this, MessageEntries);
        }

        private bool FilterNone(MessageModel arg) => true;

        private bool FilterTextAndId(MessageModel arg) =>
            $"{arg.Id.ToString()} {arg.Text}".ToLower().Contains(SearchTerm.ToLower());
    }
}
