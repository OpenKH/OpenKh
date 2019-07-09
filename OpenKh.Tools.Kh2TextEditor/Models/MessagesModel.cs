using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using OpenKh.Tools.Kh2TextEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.Kh2TextEditor.Models
{
    public class MessagesModel : GenericListModel<MessageModel>
    {
        private readonly ICurrentMessageEncoder _currentEncoder;

        public MessagesModel(ICurrentMessageEncoder currentEncoder, IEnumerable<Msg.Entry> messages) :
            this(currentEncoder, messages.Select(x => new MessageModel(currentEncoder, x)))
        { }

        public MessagesModel(ICurrentMessageEncoder currentEncoder, IEnumerable<MessageModel> messages) :
            base(messages)
        {
            _currentEncoder = currentEncoder;
        }

        public MessageModel GetMessage(int id) =>
            Items.FirstOrDefault(x => x.Id == id);

        protected override MessageModel OnNewItem()
        {
            throw new System.NotImplementedException();
        }

        internal void Filter(object p, object textFilter)
        {
            throw new NotImplementedException();
        }
    }
}
