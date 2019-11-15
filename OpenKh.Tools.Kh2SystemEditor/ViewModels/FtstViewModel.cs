using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public class Entry : BaseNotifyPropertyChanged
        {
            internal Entry(World world, int[] palette)
            {
                World = world;
                Palette = palette;
                ChangeColor = new RelayCommand<string>(strIndex =>
                {
                    var index = int.Parse(strIndex);
                    var color = ToColor(palette[index]);
                    // INSERT COLOR PICKER HERE
                    palette[index] = FromColor(color);
                    OnAllPropertiesChanged();
                }, strIndex =>
                {
                    var index = int.Parse(strIndex);
                    return index >= 0 && index < palette.Length;
                });
            }

            public World World { get; }
            public int[] Palette { get; }
            public string Name => Constants.WorldNames[(int)World];

            public Brush Color1 => ToBrush(Palette[0]);
            public Brush Color2 => ToBrush(Palette[1]);
            public Brush Color3 => ToBrush(Palette[2]);
            public Brush Color4 => ToBrush(Palette[3]);
            public Brush Color5 => ToBrush(Palette[4]);
            public Brush Color6 => ToBrush(Palette[5]);
            public Brush Color7 => ToBrush(Palette[6]);
            public Brush Color8 => ToBrush(Palette[7]);
            public Brush Color9 => ToBrush(Palette[8]);

            public ICommand ChangeColor { get; }

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
            private static Brush ToBrush(int color) => new SolidColorBrush(ToColor(color));
        }

        private const string _entryName = "ftst";

        public string EntryName => _entryName;

        public IEnumerable<Ftst.Entry> Palette => Map(this.ToArray());

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
            base(Map(ftsts.ToArray()))
        {

        }

        public Stream CreateStream()
        {
            var stream = new MemoryStream();
            Ftst.Write(stream, Palette.ToList());
            return stream;
        }

        private static IEnumerable<Entry> Map(Ftst.Entry[] entries) =>
            Enumerable.Range(0, Constants.WorldCount)
                .Select(x => new Entry((World)x, GetPalette(x, entries)));

        private static IEnumerable<Ftst.Entry> Map(Entry[] entries) =>
            Enumerable.Range(0, Constants.PaletteCount)
                .Select(x => new Ftst.Entry
                {
                    Id = x,
                    Colors = entries.Select(worldPalette => worldPalette.Palette[x]).ToArray()
                });

        private static int[] GetPalette(int index, Ftst.Entry[] entries) =>
            entries.Select(x => x.Colors[index]).ToArray();
    }
}
