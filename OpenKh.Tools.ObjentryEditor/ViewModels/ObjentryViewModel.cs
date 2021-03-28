using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Models;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.ObjentryEditor.ViewModels
{
    public class ObjentryViewModel : GenericListModel<ObjentryViewModel.ObjentryEntryViewModel>
    {
        public class ObjentryEntryViewModel : BaseNotifyPropertyChanged
        {
            public Objentry Objentry { get; }

            public ObjentryEntryViewModel(Objentry objEntry)
            {
                Objentry = objEntry;
            }

            public string Name => $"{Id} {ModelName}";

            public string Id => $"{Objentry.ObjectId:X02}";

            public uint ObjectId
            {
                get => Objentry.ObjectId;
                set { Objentry.ObjectId = value; OnPropertyChanged(nameof(Name)); }
            }

            public Objentry.Type ObjectType
            {
                get => Objentry.ObjectType;
                set
                {
                    Objentry.ObjectType = value;
                    OnPropertyChanged(nameof(ObjectType));
                }
            }
            public byte SubType { get => Objentry.SubType; set => Objentry.SubType = value; }
            public byte DrawPriority { get => Objentry.DrawPriority; set => Objentry.DrawPriority = value; }
            public byte WeaponJoint { get => Objentry.WeaponJoint; set => Objentry.WeaponJoint = value; }
            public string ModelName { get => Objentry.ModelName; set => Objentry.ModelName = value; }
            public string AnimationName { get => Objentry.AnimationName; set => Objentry.AnimationName = value; }
            public ushort Flag { get => Objentry.Flag; set => Objentry.Flag = value; }
            public byte TargetType { get => Objentry.TargetType; set => Objentry.TargetType = value; }
            public ushort NeoStatus { get => Objentry.NeoStatus; set => Objentry.NeoStatus = value; }
            public ushort NeoMoveset { get => Objentry.NeoMoveset; set => Objentry.NeoMoveset = value; }
            public float Weight { get => Objentry.Weight; set => Objentry.Weight = value; }
            public byte SpawnLimiter { get => Objentry.SpawnLimiter; set => Objentry.SpawnLimiter = value; }
            public byte Page { get => Objentry.Page; set => Objentry.Page = value; }
            public byte ShadowSize { get => Objentry.ShadowSize; set => Objentry.ShadowSize = value; }
            public Objentry.CommandMenuOptions CommandMenuOption
            {
                get => Objentry.CommandMenuOption;
                set
                {
                    Objentry.CommandMenuOption = value;
                    OnPropertyChanged(nameof(CommandMenuOption));
                }
            }
            public ushort SpawnObject1 { get => Objentry.SpawnObject1; set => Objentry.SpawnObject1 = value; }
            public ushort SpawnObject2 { get => Objentry.SpawnObject2; set => Objentry.SpawnObject2 = value; }
            public ushort SpawnObject3 { get => Objentry.SpawnObject3; set => Objentry.SpawnObject3 = value; }
            public ushort SpawnObject4 { get => Objentry.SpawnObject4; set => Objentry.SpawnObject4 = value; }

            public override string ToString() => Name;
        }


        private string _searchTerm;

        public EnumModel<Objentry.Type> ObjEntryTypes { get; }
        public EnumModel<Objentry.CommandMenuOptions> CommandMenuOptions { get; }

        public ObjentryViewModel(IEnumerable<Objentry> items) :
            base(items.Select(Map))
        {
            ObjEntryTypes = new EnumModel<Objentry.Type>();
            CommandMenuOptions = new EnumModel<Objentry.CommandMenuOptions>();
            AddAndSelectCommand = new RelayCommand(x =>
            {
                AddCommand.Execute(null);
                SelectedIndex = Items.Count - 1;
            });

            CloneCommand = new RelayCommand(x =>
            {
                var clonedItem = Clone(SelectedItem.Objentry);
                Items.Add(new ObjentryEntryViewModel(clonedItem));
                OnPropertyChanged(nameof(Items));
            }, x => SelectedItem != null);

            ClearObject1 = new RelayCommand(x =>
            {
                SelectedItem.SpawnObject1 = 0;
                OnPropertyChanged(nameof(SelectedItem));
            });

            ClearObject2 = new RelayCommand(x =>
            {
                SelectedItem.SpawnObject2 = 0;
                OnPropertyChanged(nameof(SelectedItem));
            });

            ClearObject3 = new RelayCommand(x =>
            {
                SelectedItem.SpawnObject3 = 0;
                OnPropertyChanged(nameof(SelectedItem));
            });

            ClearObject4 = new RelayCommand(x =>
            {
                SelectedItem.SpawnObject4 = 0;
                OnPropertyChanged(nameof(SelectedItem));
            });
        }

        public RelayCommand AddAndSelectCommand { get; set; }
        public RelayCommand CloneCommand { get; set; }

        public RelayCommand ClearObject1 { get; set; }
        public RelayCommand ClearObject2 { get; set; }
        public RelayCommand ClearObject3 { get; set; }
        public RelayCommand ClearObject4 { get; set; }

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

        public IEnumerable<Objentry> AsObjEntries() => Items.Select(x => x.Objentry);

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

        protected override ObjentryEntryViewModel OnNewItem()
        {
            return new ObjentryEntryViewModel(new Objentry()
            {
                ObjectId = GetObjectIdForNewEntry()
            });
        }

        private Objentry Clone(Objentry source)
        {
            var newObj = Activator.CreateInstance<Objentry>();
            foreach (var field in newObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                field.SetValue(newObj, field.GetValue(source));
            }

            newObj.ObjectId = GetObjectIdForNewEntry();
            return newObj;
        }

        private ushort GetObjectIdForNewEntry()
        {
            return (ushort)(Items.LastOrDefault()?.ObjectId + 1 ?? 0);
        }

        private bool FilterNone(ObjentryEntryViewModel arg) => true;

        private bool FilterByCharacter(ObjentryEntryViewModel arg) =>
            arg.Name.ToUpper().Contains(SearchTerm.ToUpper());

        private static ObjentryEntryViewModel Map(Objentry item) =>
            new ObjentryEntryViewModel(item);
    }
}
