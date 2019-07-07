using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.Kh2TextEditor.Models
{
    public class MessagesModel : GenericListModel<MessageModel>
    {
        private readonly IMessageEncoder _encoder;

        public MessagesModel(IMessageEncoder encoder, IEnumerable<Msg.Entry> messages) :
            this(encoder, messages.Select(x => new MessageModel(encoder, x)))
        { }

        public MessagesModel(IMessageEncoder encoder, IEnumerable<MessageModel> messages) :
            base(messages)
        {
            _encoder = encoder;
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
