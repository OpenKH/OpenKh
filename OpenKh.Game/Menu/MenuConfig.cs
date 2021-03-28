using OpenKh.Engine.Input;
using OpenKh.Engine.Renderers;
using OpenKh.Game.Infrastructure;
using System;
using System.Collections.Generic;

namespace OpenKh.Game.Menu
{
    public class MenuConfig : MenuBase
    {
        private enum Options : int
        {
            FieldCamera,
            RightStick,
            CameraUpDown,
            CameraLeftRight,
            SummonEffects,
            NavigationalMap,
            Vibration,
            Sound,
            CommandMenu,
            GameMode,

            END
        }

        private const int MaimumOptionPerLine = 3;
        private const int MaimumSettingsPerScreen = 9;
        private const int SettingCount = (int)Options.END;

        private struct OptionAnimation
        {
            public int UnselectedOption;
            public int SelectedOption;
            public int Cursor;
        }

        private struct OptionRow
        {
            public ushort Title;
            public OptionEntry[] Options;
        }

        private struct OptionEntry
        {
            public ushort Name;
            public ushort Desc;

            public OptionEntry(ushort name, ushort desc)
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

        private static readonly OptionRow[] Rows = new OptionRow[SettingCount]
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
                    new OptionEntry(0x3752, 0x372d),
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
                    new OptionEntry(0x3734, 0x3736),
                    new OptionEntry(0x3735, 0x3737),
                }
            },
            new OptionRow
            {
                Title = 0x371d,
                Options = new OptionEntry[]
                {
                    new OptionEntry(0x3738, 0x373b),
                    new OptionEntry(0x3739, 0x373c),
                    new OptionEntry(0x373a, 0x373d),
                    new OptionEntry(0x4e30, 0x4e31),
                }
            },
        };

        private readonly int[] _optionValues = new int[SettingCount];
        private IAnimatedSequence _menuSeq;
        private int _selectedOption;

        public override ushort MenuNameId => 0xb617;
        protected override bool IsEnd => _menuSeq.IsEnd;

        public int SelectedSettingIndex
        {
            get => _selectedOption;
            private set
            {
                value = value < 0 ? SettingCount - 1 : value % SettingCount;
                if (_selectedOption != value)
                {
                    _selectedOption = value;
                    InvalidateMenu();
                }
            }
        }

        public MenuConfig(IMenuManager menuManager) : base(menuManager)
        {
            _menuSeq = InitializeMenu(false);
            _menuSeq.Begin();

            SetSelectedOption((int)Options.FieldCamera, menuManager.GameContext.Kernel.SaveData.FieldCameraManual ? 1 : 0);
            SetSelectedOption((int)Options.RightStick, menuManager.GameContext.Kernel.SaveData.RightAnalogStickCommand ? 1 : 0);
            SetSelectedOption((int)Options.CameraUpDown, menuManager.GameContext.Kernel.SaveData.CameraUpDownReversed ? 1 : 0);
            SetSelectedOption((int)Options.CameraLeftRight, menuManager.GameContext.Kernel.SaveData.CameraLeftRightReversed ? 1 : 0);
            SetSelectedOption((int)Options.SummonEffects, 0); // TODO not implemented
            SetSelectedOption((int)Options.NavigationalMap, menuManager.GameContext.Kernel.SaveData.NavigationalMap ? 0 : 1);
            SetSelectedOption((int)Options.Vibration, menuManager.GameContext.Kernel.SaveData.Vibration ? 0 : 1);
            SetSelectedOption((int)Options.Sound, 0); // TODO not implemented
            SetSelectedOption((int)Options.CommandMenu, menuManager.GameContext.Kernel.SaveData.CommandMenuClassic ? 1 : 0);
            SetSelectedOption((int)Options.GameMode, menuManager.GameContext.Kernel.SaveData.Difficulty);
        }

        public int GetOptionCount(int settingIndex) => Rows[settingIndex].Options.Length;
        public int GetSelectedOption(int settingIndex) => _optionValues[settingIndex];
        public void SetSelectedOption(int settingIndex, int value)
        {
            var optionCount = GetOptionCount(settingIndex);
            if (value < 0)
                value = optionCount - 1;
            else
                value %= optionCount;

            if (_optionValues[settingIndex] != value)
            {
                _optionValues[settingIndex] = value;
                InvalidateMenu();
            }
        }

        private void InvalidateMenu()
        {
            _menuSeq = InitializeMenu(true);
            _menuSeq.Begin();
        }

        private IAnimatedSequence InitializeMenu(bool skipIntro = false)
        {
            var settingList = new List<AnimatedSequenceDesc>();
            var displaySettingStart = Math.Max(SelectedSettingIndex - MaimumSettingsPerScreen + 1, 0);
            var displaySettingCount = Math.Min(SettingCount, MaimumSettingsPerScreen);

            for (var i = displaySettingStart; i < displaySettingStart + displaySettingCount; i++)
            {
                var setting = Rows[i];
                var optionAnimations = OptionAnimations[setting.Options.Length switch
                {
                    2 => 1,
                    3 => 2,
                    _ => 0,
                }];
                var displayOptionCount = setting.Options.Length <= MaimumOptionPerLine ?
                    setting.Options.Length : 1;

                var optionList = new List<AnimatedSequenceDesc>();
                for (var j = 0; j < displayOptionCount; j++)
                {
                    if (j == GetSelectedOption(i))
                    {
                        if (i == SelectedSettingIndex)
                        {
                            optionList.Add(new AnimatedSequenceDesc
                            {
                                SequenceIndexLoop = optionAnimations.SelectedOption,
                                TextAnchor = TextAnchor.BottomCenter,
                                MessageId = setting.Options[j].Name,
                                Children = new List<AnimatedSequenceDesc>
                                {
                                    new AnimatedSequenceDesc
                                    {
                                        SequenceIndexLoop = optionAnimations.Cursor,
                                        Flags = AnimationFlags.TextTranslateX |
                                            AnimationFlags.ChildStackHorizontally,
                                        Children = new List<AnimatedSequenceDesc>
                                        {
                                            new AnimatedSequenceDesc
                                            {
                                                SequenceIndexLoop = 25,
                                                TextAnchor = TextAnchor.BottomLeft,
                                                Flags = AnimationFlags.NoChildTranslationX,
                                            },
                                            new AnimatedSequenceDesc
                                            {
                                                SequenceIndexStart = skipIntro ? -1 : 27,
                                                SequenceIndexLoop = 28,
                                                SequenceIndexEnd = 29,
                                            }
                                        }
                                    }
                                }
                            });
                        }
                        else
                        {
                            optionList.Add(new AnimatedSequenceDesc
                            {
                                SequenceIndexLoop = optionAnimations.SelectedOption,
                                TextAnchor = TextAnchor.BottomCenter,
                                MessageId = setting.Options[j].Name,
                            });
                        }
                    }
                    else
                    {
                        optionList.Add(new AnimatedSequenceDesc
                        {
                            SequenceIndexLoop = optionAnimations.UnselectedOption,
                            TextAnchor = TextAnchor.BottomCenter,
                            MessageId = setting.Options[j].Name,
                        });
                    }
                }

                settingList.Add(new AnimatedSequenceDesc
                {
                    SequenceIndexLoop = 279,
                    MessageId = setting.Title,
                    TextAnchor = TextAnchor.BottomCenter,
                    StackIndex = i,
                    Flags = AnimationFlags.NoChildTranslationX |
                        AnimationFlags.TextTranslateX |
                        AnimationFlags.TextIgnoreColor |
                        AnimationFlags.TextIgnoreScaling |
                        AnimationFlags.StackNextChildHorizontally,
                    Children = optionList
                });
            }

            var anim = SequenceFactory.Create(new AnimatedSequenceDesc
            {
                SequenceIndexStart = skipIntro ? -1 : 233,
                SequenceIndexLoop = 234,
                SequenceIndexEnd = 235,
                Flags = AnimationFlags.StackNextChildVertically,
                Children = settingList
            });
            anim.Begin();

            return anim;
        }

        protected override void ProcessInput(IInput input)
        {
            if (input.Repeated.Up)
                SelectedSettingIndex--;
            else if (input.Repeated.Down)
                SelectedSettingIndex++;
            else if (input.Repeated.Left)
                SetSelectedOption(SelectedSettingIndex, GetSelectedOption(SelectedSettingIndex) - 1);
            else if (input.Repeated.Right)
                SetSelectedOption(SelectedSettingIndex, GetSelectedOption(SelectedSettingIndex) + 1);
            else if (input.Triggered.Confirm)
            {
                // ignore
            }
            else
                base.ProcessInput(input);
        }

        public override void Open()
        {
            _menuSeq.Begin();
            base.Open();
        }

        public override void Close()
        {
            _menuSeq.End();
            base.Close();
        }

        protected override void MyUpdate(double deltaTime) =>
            _menuSeq.Update(deltaTime);

        protected override void MyDraw() =>
            _menuSeq.Draw(0, 0);
    }
}
