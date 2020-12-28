using OpenKh.Kh2;
using OpenKh.Kh2.SystemData;
using OpenKh.Tools.Common.Models;
using OpenKh.Tools.Kh2SystemEditor.Extensions;
using OpenKh.Tools.Kh2SystemEditor.Interfaces;
using OpenKh.Tools.Kh2SystemEditor.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.Tools;

namespace OpenKh.Tools.Kh2SystemEditor.ViewModels
{
    public class MemtViewModel : MyGenericListModel<MemtViewModel.Entry>, ISystemGetChanges
    {
        public class Entry : BaseNotifyPropertyChanged
        {
            private readonly Memt.Entry _entry;
            private readonly IObjectProvider _objectProvider;

            internal Entry(Memt.Entry entry, IObjectProvider objectProvider)
            {
                _entry = entry;
                _objectProvider = objectProvider;
                Worlds = new Kh2WorldsList();
                Worlds.First().Name = "Ignore";
            }

            public string Title => WorldId == World.WorldZz ? "For any world" :
                Worlds.FirstOrDefault(x => x.Value == WorldId)?.Name;

            public Kh2WorldsList Worlds { get; }
            public IEnumerable<ObjectModel> Objects => _objectProvider.Objects;

            public World WorldId
            {
                get => (World)_entry.WorldId;
                set
                {
                    _entry.WorldId = (short)value;
                    OnPropertyChanged(nameof(Title));
                }
            }

            public short Unk06 { get => _entry.Unk06; set => _entry.Unk06 = value; }
            public short Unk08 { get => (short)(_entry.Unk08 & 0xff); set => _entry.Unk08 = value; }
            public short Unk0A { get => _entry.Unk0A; set => _entry.Unk0A = value; }
            public short Unk0C { get => (short)(_entry.Unk0C & 0xff); set => _entry.Unk0C = value; }
            public short Unk0E { get => _entry.Unk0E; set => _entry.Unk0E = value; }
            public short[] Members { get => _entry.Members; set => _entry.Members = value; }

            public World FlagWorld
            {
                get => (World)(_entry.CheckStoryFlag >> 10);
                set => _entry.CheckStoryFlag = (short)((_entry.CheckStoryFlag & 0x3FF) | ((int)value << 10));
            }

            public int FlagStory
            {
                get => _entry.CheckStoryFlag & 0x3ff;
                set => _entry.CheckStoryFlag = (short)((_entry.CheckStoryFlag & ~0x3FF) | (value & 0x3ff));
            }

            public World FlagWorldNeg
            {
                get => (World)(_entry.CheckStoryFlagNegation >> 10);
                set => _entry.CheckStoryFlagNegation = (short)((_entry.CheckStoryFlag & 0x3FF) | ((int)value << 10));
            }

            public int FlagStoryNeg
            {
                get => _entry.CheckStoryFlagNegation & 0x3ff;
                set => _entry.CheckStoryFlagNegation = (short)((_entry.CheckStoryFlag & ~0x3FF) | (value & 0x3ff));
            }
        }

        private readonly Memt _memberTable;
        private readonly List<Memt.Entry> _entries = new List<Memt.Entry>();
        private readonly IObjectProvider _objectProvider;
        private const string _entryName = "memt";

        public string EntryName => _entryName;

        public IEnumerable<Memt.Entry> Members => _entries;

        public MemtViewModel(IObjectProvider objectProvider) :
            this(objectProvider, new Memt())
        { }

        public MemtViewModel(IObjectProvider objectProvider, IEnumerable<Bar.Entry> entries) :
            this(objectProvider, Memt.Read(entries.GetBinaryStream(_entryName)))
        { }

        public MemtViewModel(IObjectProvider objectProvider, Memt memberTable) :
            base(memberTable.Entries.Select(x => new Entry(x, objectProvider)))
        {
            _objectProvider = objectProvider;
            _memberTable = memberTable;
        }

        protected override Entry OnNewItem() => new Entry(new Memt.Entry
        {
            Members = new short[18]
        }, _objectProvider);

        public Stream CreateStream()
        {
            var stream = new MemoryStream();
            Memt.Write(stream, _memberTable);
            return stream;
        }
    }
}
