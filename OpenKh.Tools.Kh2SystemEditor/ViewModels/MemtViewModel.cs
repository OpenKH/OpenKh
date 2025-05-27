using OpenKh.Kh2;
using OpenKh.Kh2.SystemData;
using OpenKh.Tools.Common.Models;
using OpenKh.Tools.Common.Wpf.Models;
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
            private readonly int[] MemberLookupVanilla =
            {
                0, 1, 2, 3, 4, 5, -1, 6, 7, 8, 9, 10, 11, 12, -1, 13, 14, 15
            };
            private readonly int[] MemberLookupFinalMix =
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17
            };

            private readonly Memt.IEntry _entry;
            private readonly IObjectProvider _objectProvider;
            private readonly int[] _memberLookup;

            internal Entry(Memt.IEntry entry, IObjectProvider objectProvider)
            {
                _entry = entry;
                _objectProvider = objectProvider;
                _memberLookup = entry switch
                {
                    Memt.EntryVanilla _ => MemberLookupVanilla,
                    Memt.EntryFinalMix _ => MemberLookupFinalMix,
                    _ => MemberLookupFinalMix,
                };

                Worlds = new Kh2WorldsList();
                Worlds.First().Name = "Ignore";
                MemberEditEnabled = _memberLookup.Select(x => x >= 0).ToArray();
            }

            public string Title => WorldId == World.WorldZz ? "For any world" :
                Worlds.FirstOrDefault(x => x.Value == WorldId)?.Name;

            public Kh2WorldsList Worlds { get; }
            public IEnumerable<ObjectModel> Objects => _objectProvider.Objects;
            public bool[] MemberEditEnabled { get; }

            public World WorldId
            {
                get => (World)_entry.WorldId;
                set
                {
                    _entry.WorldId = (short)value;
                    OnPropertyChanged(nameof(Title));
                }
            }

            public byte CheckArea { get => _entry.CheckArea; set => _entry.CheckArea = value; }
            public byte Padding { get => _entry.Padding; set => _entry.Padding = value; }
            public int PlayerSize { get => _entry.PlayerSize; set => _entry.PlayerSize = value; }
            public int FriendSize { get => _entry.FriendSize; set => _entry.FriendSize = value; }
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

            public short PLAYER { get => GetMember(0); set => SetMember(0, value); }
            public short FRIEND_1 { get => GetMember(1); set => SetMember(1, value); }
            public short FRIEND_2 { get => GetMember(2); set => SetMember(2, value); }
            public short FRIEND_W { get => GetMember(3); set => SetMember(3, value); }
            public short PLAYER_BTLF { get => GetMember(4); set => SetMember(4, value); }
            public short PLAYER_MAGF { get => GetMember(5); set => SetMember(5, value); }
            public short PLAYER_KH1F { get => GetMember(6); set => SetMember(6, value); }
            public short PLAYER_TRIF { get => GetMember(7); set => SetMember(7, value); }
            public short PLAYER_ULTF { get => GetMember(8); set => SetMember(8, value); }
            public short PLAYER_HTLF { get => GetMember(9); set => SetMember(9, value); }
            public short MICKEY { get => GetMember(10); set => SetMember(10, value); }
            public short PLAYER_H { get => GetMember(11); set => SetMember(11, value); }
            public short PLAYER_H_BTLF { get => GetMember(12); set => SetMember(12, value); }
            public short PLAYER_H_MAGF { get => GetMember(13); set => SetMember(13, value); }
            public short PLAYER_H_KH1F { get => GetMember(14); set => SetMember(14, value); }
            public short PLAYER_H_TRIF { get => GetMember(15); set => SetMember(15, value); }
            public short PLAYER_H_ULTF { get => GetMember(16); set => SetMember(16, value); }
            public short PLAYER_H_HTLF { get => GetMember(17); set => SetMember(17, value); }

            private short GetMember(int index)
            {
                var actualIndex = _memberLookup[index];
                if (actualIndex == -1)
                    return -1;
                return Members[actualIndex];
            }

            private void SetMember(int index, short value)
            {
                var actualIndex = _memberLookup[index];
                if (actualIndex == -1)
                    return;
                Members[actualIndex] = value;
            }
        }

        private readonly Memt _memberTable;
        private readonly List<Memt.IEntry> _entries = new List<Memt.IEntry>();
        private readonly IObjectProvider _objectProvider;
        private const string _entryName = "memt";

        public string EntryName => _entryName;

        public IEnumerable<Memt.IEntry> Members => _entries;

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

        protected override Entry OnNewItem()
        {
            var item = _entries.FirstOrDefault();
            if (item is Memt.EntryVanilla)
                return new Entry(new Memt.EntryVanilla
                {
                    Members = new short[Memt.MemberCountVanilla]
                }, _objectProvider);
            else
                return new Entry(new Memt.EntryFinalMix
                {
                    Members = new short[Memt.MemberCountFinalMix]
                }, _objectProvider);
        }

        public Stream CreateStream()
        {
            var stream = new MemoryStream();
            Memt.Write(stream, _memberTable);
            return stream;
        }
    }
}
