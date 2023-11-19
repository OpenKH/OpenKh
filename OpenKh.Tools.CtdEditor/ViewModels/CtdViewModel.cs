using OpenKh.Bbs;
using OpenKh.Tools.CtdEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.CtdEditor.ViewModels
{
    public class CtdViewModel : GenericListModel<MessageViewModel>
    {
        private string _searchTerm;
        private readonly Ctd _ctd;
        private readonly IDrawHandler _drawHandler;

        public Ctd Ctd
        {
            get
            {
                _ctd.Messages.Clear();
                _ctd.Messages.AddRange(Items.Select(x => x.Message));
                return _ctd;
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

        public uint CtdID
        {
            get => _ctd.FileID;
            set => _ctd.FileID = value;
        }

        public CtdViewModel(IDrawHandler drawHandler) :
            this(drawHandler, new Ctd())
        {
        }

        public CtdViewModel(IDrawHandler drawHandler, Ctd ctd) :
            this(drawHandler, ctd, ctd.Messages)
        {
            _drawHandler = drawHandler;
        }

        private CtdViewModel(IDrawHandler drawHandler, Ctd ctd, IEnumerable<Ctd.Message> messages) :
            base(messages.Select(x => new MessageViewModel(drawHandler, ctd, x)))
        {
            _ctd = ctd;
        }

        protected override void OnSelectedItem(MessageViewModel item)
        {
            base.OnSelectedItem(item);
        }

        protected override MessageViewModel OnNewItem() =>
            new MessageViewModel(_drawHandler, _ctd, new Ctd.Message
            {
                Id = (Items.Max(x => x.Message.Id) + 1),
                Data = new byte[0],
                LayoutIndex = 0,
                WaitFrames = 0
            });

        private void PerformFiltering()
        {
            if (string.IsNullOrWhiteSpace(_searchTerm))
                Filter(FilterNone);
            else
                Filter(FilterTextAndId);
        }

        private bool FilterNone(MessageViewModel arg) => true;

        private bool FilterTextAndId(MessageViewModel arg) =>
            $"{arg.Id} {arg.Text}".ToLower().Contains(SearchTerm.ToLower());
    }
}
