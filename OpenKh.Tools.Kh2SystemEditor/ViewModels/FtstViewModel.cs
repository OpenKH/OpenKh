using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using OpenKh.Kh2;
using OpenKh.Kh2.System;
using OpenKh.Tools.Common;
using OpenKh.Tools.Common.Models;
using OpenKh.Tools.Kh2SystemEditor.Extensions;
using OpenKh.Tools.Kh2SystemEditor.Interfaces;
using Xe.Tools;

namespace OpenKh.Tools.Kh2SystemEditor.ViewModels
{
    public class FtstViewModel : MyGenericListModel<FtstViewModel.Entry>, ISystemGetChanges
    {
        public class ColorItem : BaseNotifyPropertyChanged
        {
            public Color? CurrentColor
            {
                get => ToColor(GetColor());
                set => SetColor(FromColor(value ?? Colors.Black));
            }

            internal Func<int> GetColor;
            internal Action<int> SetColor;

            private static int FromColor(Color color)
            {
                var newColor = ((color.A + 1) / 2) << 24;
                newColor |= color.R << 16;
                newColor |= color.G << 8;
                newColor |= color.B;

                return newColor;
            }
            private static Color ToColor(int color)
            {
                var ch1 = (byte)((color >> 16) & 0xFF);
                var ch2 = (byte)((color >> 8) & 0xFF);
                var ch3 = (byte)((color >> 0) & 0xFF);
                var ch4 = (byte)(((color >> 24) & 0xFF) * 2 - 1);
                return Color.FromArgb(ch4, ch1, ch2, ch3);
            }
        }

        public class Entry : BaseNotifyPropertyChanged
        {
            internal Entry(World world, int numColors, Func<int, int> getter, Action<int, int> setter)
            {
                World = world;

                for (var loop = 0; loop < numColors; loop++)
                {
                    var colorIndex = loop;

                    ColorItems.Add(
                        new ColorItem
                        {
                            GetColor = () => getter(colorIndex),
                            SetColor = (value) => setter(colorIndex, value)
                        }
                    );
                }
            }

            public World World { get; }

            public string Name => Constants.WorldNames[(int)World];

            public List<ColorItem> ColorItems { get; } = new List<ColorItem>();
        }

        private readonly List<Ftst.Entry> _entries = new List<Ftst.Entry>();

        private const string _entryName = "ftst";

        public string EntryName => _entryName;

        public IEnumerable<Ftst.Entry> Palette => _entries;

        public FtstViewModel() :
            this(Enumerable.Range(0, Constants.PaletteCount).Select(x => new Ftst.Entry
            {
                Id = x,
                Colors = new int[Constants.WorldCount]
            }))
        { }

        public FtstViewModel(IEnumerable<Bar.Entry> entries) :
            this(Ftst.Read(entries.GetBinaryStream(_entryName)))
        { }

        public FtstViewModel(IEnumerable<Ftst.Entry> ftsts) :
            base(new Entry[0])
        {
            _entries.AddRange(ftsts);

            var maxCount = ftsts.Count();

            for (var loop = 0; loop < Constants.WorldCount; loop++)
            {
                var worldIndex = loop;

                Items.Add(
                    new Entry(
                        (World)worldIndex,
                        maxCount,
                        (index) => _entries[index].Colors[worldIndex],
                        (index, value) => _entries[index].Colors[worldIndex] = value
                    )
                );
            }
        }

        public Stream CreateStream()
        {
            var stream = new MemoryStream();
            Ftst.Write(stream, Palette.ToList());
            return stream;
        }
    }
}
