using OpenKh.Kh2;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.ObjentryEditor.ViewModels
{
    public class ObjentryViewModel : GenericListModel<ObjentryViewModel.ObjentryEntryViewModel>
    {
        public class ObjentryEntryViewModel
        {
            public Objentry Objentry { get; }

            public ObjentryEntryViewModel(Objentry entry)
            {
                Objentry = entry;
            }

            public string Name => $"{Id} {ModelName}";

            public string Id => $"{Objentry.ObjectId:X02}";

            public ushort ObjectId { get => Objentry.ObjectId; set => Objentry.ObjectId = value; }
            public ushort Unknown02 { get => Objentry.Unknown02; set => Objentry.Unknown02 = value; }
            public ushort ObjectType { get => Objentry.ObjectType; set => Objentry.ObjectType = value; }
            public ushort ApparationPriority { get => Objentry.ApparationPriority; set => Objentry.ApparationPriority = value; }
            public string ModelName { get => Encoding.Default.GetString(Objentry.ModelName); set => Objentry.ModelName = Encoding.Default.GetBytes(value); }
            public string AnimationName { get => Encoding.Default.GetString(Objentry.AnimationName); set => Objentry.AnimationName = Encoding.Default.GetBytes(value); }
            public uint Unknown48 { get => Objentry.Unknown48; set => Objentry.Unknown48 = value; }
            public ushort LoadedVsb { get => Objentry.LoadedVsb; set => Objentry.LoadedVsb = value; }
            public ushort LoadedWeapon { get => Objentry.LoadedWeapon; set => Objentry.LoadedWeapon = value; }
            public uint CameraPosition { get => Objentry.CameraPosition; set => Objentry.CameraPosition = value; }
            public uint Unknown58 { get => Objentry.Unknown58; set => Objentry.Unknown58 = value; }
            public uint Unknown5c { get => Objentry.Unknown5c; set => Objentry.Unknown5c = value; }

            public override string ToString() => Name;
        }

        private const int DefaultType = 3;

        private readonly int _type;
        private string _searchTerm;

        public ObjentryViewModel(BaseTable<Objentry> objentry) :
            this(objentry.Id, objentry.Items)
        { }

        public ObjentryViewModel(int type, IEnumerable<Objentry> items) :
            base(items.Select(Map))
        {
            _type = type;
        }


        public Visibility IsItemEditingVisible => IsItemSelected ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsItemEditMessageVisible => !IsItemSelected ? Visibility.Visible : Visibility.Collapsed;

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                PerformFiltering();
            }
        }

        public Stream CreateStream()
        {
            var stream = new MemoryStream();
            new BaseTable<Objentry>
            {
                Id = _type,
                Items = Items.Select(x => x.Objentry).ToList()
            }.Write(stream);

            return stream;
        }

        protected override void OnSelectedItem(ObjentryEntryViewModel item)
        {
            base.OnSelectedItem(item);

            OnPropertyChanged(nameof(IsItemEditingVisible));
            OnPropertyChanged(nameof(IsItemEditMessageVisible));
        }

        private void PerformFiltering()
        {
            if (string.IsNullOrWhiteSpace(_searchTerm))
                Filter(FilterNone);
            else
                Filter(FilterByCharacter);
        }

        private bool FilterNone(ObjentryEntryViewModel arg) => true;

        private bool FilterByCharacter(ObjentryEntryViewModel arg) =>
            arg.Name.ToUpper().Contains(SearchTerm.ToUpper());

        private static ObjentryEntryViewModel Map(Objentry item) =>
            new ObjentryEntryViewModel(item);
    }
}
