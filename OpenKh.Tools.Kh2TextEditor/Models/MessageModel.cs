using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using OpenKh.Tools.Kh2TextEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Xe.Tools;

namespace OpenKh.Tools.Kh2TextEditor.Models
{
    public class MessageModel : BaseNotifyPropertyChanged
    {
        private readonly ICurrentMessageEncoder _currentEncoder;
        private readonly IInvalidateErrorCount _invalidateErrorCount;
        private readonly Msg.Entry _entry;
        private bool _doesNotContainErrors = true;
        private string _lastError;

        public MessageModel(ICurrentMessageEncoder currentEncoder, IInvalidateErrorCount invalidateErrorCount, Msg.Entry entry)
        {
            _currentEncoder = currentEncoder;
            _invalidateErrorCount = invalidateErrorCount;
            _entry = entry;
        }

        public int Id => _entry.Id;

        public string Text
        {
            get
            {
                try
                {
                    var text = MsgSerializer.SerializeText(MessageCommands);
                    DoesNotContainErrors = DoesNotContainErrors && true;
                    return text;
                }
                catch (Exception ex)
                {
                    DoesNotContainErrors = false;
                    LastError = ex.Message;
                    return ex.Message;
                }
            }

            set
            {
                try
                {
                    MessageCommands = MsgSerializer.DeserializeText(value);
                    DoesNotContainErrors = true;
                }
                catch (Exception ex)
                {
                    DoesNotContainErrors = false;
                    LastError = ex.Message;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public IEnumerable<MessageCommandModel> MessageCommands
        {
            get => _currentEncoder.CurrentMessageEncoder.Decode(_entry.Data);
            set
            {
                _entry.Data = _currentEncoder.CurrentMessageEncoder.Encode(value.ToList());
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get
            {
                const int MaxTitleLength = 100;
                var title = $"{Id:X4}: {Text}";
                return title.Length > MaxTitleLength ? $"{title.Substring(0, MaxTitleLength)}..." : title;
            }
        }

        public bool DoesNotContainErrors
        {
            get => _doesNotContainErrors;
            private set
            {
                var prevValue = _doesNotContainErrors;
                _doesNotContainErrors = value;

                if (prevValue != value)
                    _invalidateErrorCount.InvalidateErrorCount();

                OnPropertyChanged(nameof(IconErrorVisiblity));
            }
        }

        public Visibility IconErrorVisiblity => DoesNotContainErrors ? Visibility.Collapsed : Visibility.Visible;

        public string LastError
        {
            get => _lastError;
            private set
            {
                _lastError = value;
                OnPropertyChanged();
            }
        }

        public void InvalidateText()
        {
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(Text));
        }
    }
}
