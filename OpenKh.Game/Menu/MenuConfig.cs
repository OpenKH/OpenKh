namespace OpenKh.Game.Menu
{
    public class MenuConfig : IMenu
    {
        private const int MaimumOptionPerLine = 3;
        private const int OptionCount = 10;

        private struct OptionAnimation
        {
            public int UnselectedOption;
            public int SelectedOption;
            public int Cursor;
        }

        private struct OptionRow
        {
            public int Title;
            public OptionEntry[] Options;
        }

        private struct OptionEntry
        {
            public int Name;
            public int Desc;

            public OptionEntry(int name, int desc)
            {
                Name = name;
                Desc = desc;
            }
        }

        private static readonly int[] Names = new int[]
        {
            0x3717,
            0x3718,
            0x42f5,
            0x42f6,
            0x42f7,
            0x3719,
            0x371a,
            0x371b,
            0x371c,
            0x371d,
        };

        private static readonly OptionAnimation[] OptionAnimations = new OptionAnimation[MaimumOptionPerLine]
        {
            new OptionAnimation { UnselectedOption = 261, SelectedOption = 260, Cursor = 259 },
            new OptionAnimation { UnselectedOption = 254, SelectedOption = 255, Cursor = 253 },
            new OptionAnimation { UnselectedOption = 257, SelectedOption = 258, Cursor = 256 },
        };

        private static readonly OptionRow[] Rows = new OptionRow[OptionCount]
        {
            new OptionRow
            {
                Title = 0x3717,
                Options = new OptionEntry[]
                {
                    new OptionEntry(0x371e, 0x3720),
                    new OptionEntry(0x371f, 0x3721),
                }
            },
            new OptionRow
            {
                Title = 0x3718,
                Options = new OptionEntry[]
                {
                    new OptionEntry(0x3722, 0x3724),
                    new OptionEntry(0x3723, 0x3725),
                }
            },
            new OptionRow
            {
                Title = 0x42f5,
                Options = new OptionEntry[]
                {
                    new OptionEntry(0x42f8, 0x42fa),
                    new OptionEntry(0x42f9, 0x42fb),
                }
            },
            new OptionRow
            {
                Title = 0x42f6,
                Options = new OptionEntry[]
                {
                    new OptionEntry(0x42fc, 0x42fe),
                    new OptionEntry(0x42fd, 0x42ff),
                }
            },
            new OptionRow
            {
                Title = 0x42f7,
                Options = new OptionEntry[]
                {
                    new OptionEntry(0x4302, 0x4305),
                    new OptionEntry(0x4300, 0x4303),
                    new OptionEntry(0x4301, 0x4304),
                }
            },
            new OptionRow
            {
                Title = 0x3719,
                Options = new OptionEntry[]
                {
                    new OptionEntry(0x3726, 0x3728),
                    new OptionEntry(0x3727, 0x3729),
                }
            },
            new OptionRow
            {
                Title = 0x371a,
                Options = new OptionEntry[]
                {
                    new OptionEntry(0x372a, 0x372c),
                    new OptionEntry(0x372b, 0x372d),
                }
            },
            new OptionRow
            {
                Title = 0x371b,
                Options = new OptionEntry[]
                {
                    new OptionEntry(0x372e, 0x3731),
                    new OptionEntry(0x372f, 0x3732),
                    new OptionEntry(0x3730, 0x3733),
                }
            },
            new OptionRow
            {
                Title = 0x371c,
                Options = new OptionEntry[]
                {
                    new OptionEntry(0x3724, 0x3736),
                    new OptionEntry(0x3725, 0x3737),
                }
            },
            new OptionRow
            {
                Title = 0x371d,
                Options = new OptionEntry[]
                {
                    new OptionEntry(0x4e30, 0x4e31),
                }
            },
        };

        public int SelectedOption { get; private set; }

        public bool IsClosed { get; private set; }

        public void Open()
        {

        }

        public void Close()
        {
        }

        public void Push(IMenu menu)
        { }

        public void Update(double deltaTime)
        {
        }

        public void Draw()
        {
        }
    }
}
