using OpenKh.Engine.Renderers;
using OpenKh.Game.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.Menu
{
    public class MainMenu : MenuBase
    {
        private const int MaxCharacterCount = 4;
        private const int MaxMenuElementCount = 8;
        private const int CharacterHpBar = 98;
        private const int CharacterMpBar = 99;
        private const int MsgLv = 0x39FC;
        private const int MsgHp = 0x39FD;
        private const int MsgMp = 0x39FE;
        private static readonly ushort[] MenuOptions = new ushort[MaxMenuElementCount]
        {
            0x844b, // Items
            0x844d, // Abilities
            0x8451, // Customize
            0x844e, // Party
            0x844f, // Status
            0x8450, // Journal
            0x8450, // Journal
            0xb617, // Config
        };
        private static readonly ushort[] CharacterNames = new ushort[MaxCharacterCount]
        {
            0x851f, // Sora
            0x8520, // Donald
            0x8521, // Goofy
            0x852c, // Riku
        };

        private IAnimatedSequence _backgroundSeq;
        private IAnimatedSequence _menuSeq;
        private IAnimatedSequence _characterSeq;

        private int _optionCount = 0;
        private int _optionSelected = 0;
        private bool _isDebugMenuVisible;

        public override ushort MenuNameId => 0;
        protected override bool IsEnd =>
            _backgroundSeq.IsEnd &&
            _menuSeq.IsEnd &&
            _characterSeq.IsEnd;

        public int SelectedOption
        {
            get => _optionSelected;
            set
            {
                if (_optionCount == 0)
                    return;

                _optionSelected = value;
                if (_optionSelected < 0)
                    _optionSelected += _optionCount;
                _optionSelected %= _optionCount;

                InvalidateMenu();
            }
        }

        public bool IsDebugMenuVisible
        {
            get => _isDebugMenuVisible;
            set
            {
                if (_isDebugMenuVisible != value)
                {
                    _isDebugMenuVisible = value;
                    InvalidateMenu();
                }
            }
        }

        public MainMenu(IMenuManager menuManager) : base(menuManager)
        {
            InitializeMenu();
        }

        private void InvalidateMenu()
        {
            var frame = _menuSeq.FrameIndex;
            _menuSeq = InitializeMenuOptions(true);
            _menuSeq.Begin();
            _menuSeq.FrameIndex = frame;
        }

        private void InitializeMenu()
        {
            _backgroundSeq = SequenceFactory.Create(new AnimatedSequenceDesc
            {
                SequenceIndexStart = 46,
                SequenceIndexLoop = 47,
                SequenceIndexEnd = 48,
            });
            _menuSeq = InitializeMenuOptions();

            _characterSeq = SequenceFactory.Create(Enumerable.Range(0, 5)
                .Select(i => new AnimatedSequenceDesc
                {
                    SequenceIndexStart = 101,
                    SequenceIndexLoop = 102,
                    SequenceIndexEnd = 103,
                    StackIndex = i + 1,
                    StackWidth = AnimatedSequenceDesc.DefaultStacking,
                    Children = new List<AnimatedSequenceDesc>()
                    {
                        new AnimatedSequenceDesc
                        {
                            SequenceIndexLoop = 93,
                            Children = new List<AnimatedSequenceDesc>()
                            {
                                new AnimatedSequenceDesc
                                {
                                    SequenceIndexLoop = 124,
                                    MessageText = "Donald",
                                    Flags = AnimationFlags.TextIgnoreColor,
                                    TextAnchor = TextAnchor.BottomCenter,
                                },
                                new AnimatedSequenceDesc()
                                {
                                    SequenceIndexLoop = 90,
                                    Children = new List<AnimatedSequenceDesc>()
                                    {
                                        new AnimatedSequenceDesc
                                        {
                                            SequenceIndexLoop = 124,
                                            MessageId = MsgLv,
                                            TextAnchor = TextAnchor.BottomLeft,
                                        },
                                        new AnimatedSequenceDesc
                                        {
                                            SequenceIndexLoop = 124,
                                            MessageText = "99",
                                            TextAnchor = TextAnchor.BottomRight,
                                        },
                                        new AnimatedSequenceDesc()
                                        {
                                            StackIndex = 1,
                                            SequenceIndexLoop = 121,
                                            MessageId = MsgHp,
                                            TextAnchor = TextAnchor.BottomLeft,
                                        },
                                        new AnimatedSequenceDesc()
                                        {
                                            StackIndex = 1,
                                            SequenceIndexLoop = 121,
                                            MessageText = "60/60",
                                            TextAnchor = TextAnchor.BottomRight,
                                        },
                                        new AnimatedSequenceDesc()
                                        {
                                            StackIndex = 1,
                                            SequenceIndexLoop = CharacterHpBar,
                                            TextAnchor = TextAnchor.BottomLeft,
                                        },
                                        new AnimatedSequenceDesc()
                                        {
                                            StackIndex = 2,
                                            SequenceIndexLoop = 118,
                                            MessageId = MsgMp,
                                            TextAnchor = TextAnchor.BottomLeft,
                                        },
                                        new AnimatedSequenceDesc()
                                        {
                                            StackIndex = 2,
                                            SequenceIndexLoop = 118,
                                            MessageText = "120/120",
                                            TextAnchor = TextAnchor.BottomRight,
                                        },
                                        new AnimatedSequenceDesc()
                                        {
                                            StackIndex = 2,
                                            SequenceIndexLoop = CharacterMpBar,
                                            TextAnchor = TextAnchor.BottomLeft,
                                        },
                                    }
                                },
                            }
                        }
                    }
                })
            );
        }

        protected override void ProcessInput(InputManager inputManager)
        {
            if (inputManager.IsMenuUp)
                SelectedOption--;
            if (inputManager.IsMenuDown)
                SelectedOption++;
            if (inputManager.IsCircle)
            {
                switch (SelectedOption)
                {
                    case 6:
                        if (_isDebugMenuVisible)
                            Push(new MenuDebug(MenuManager));
                        else
                            Push(new MenuConfig(MenuManager));
                        break;
                    default:
                        Push(new MenuTemplate(MenuManager, 0));
                        break;
                }
            }
            else if (inputManager.IsCross)
                MenuManager.CloseAllMenu();

            IsDebugMenuVisible = inputManager.RightTrigger;
        }

        private IAnimatedSequence InitializeMenuOptions(bool skipIntro = false)
        {
            const int MenuOptionsBitfields = 0xbf;
            var menuDesc = new List<AnimatedSequenceDesc>();
            var menuOptions = MenuOptionsBitfields;

            _optionCount = 0;
            for (int bitIndex = 0; menuOptions > 0; bitIndex++)
            {
                var bitMask = 1 << bitIndex;
                if ((menuOptions & bitMask) == 0)
                    continue;
                menuOptions -= bitMask;
                if (_optionCount >= MenuOptions.Length)
                    break;

                string overrideTextField = null;
                if (bitIndex == 7 && _isDebugMenuVisible)
                    overrideTextField = MenuDebug.Name;

                AnimatedSequenceDesc desc = _optionCount != _optionSelected
                    ? new AnimatedSequenceDesc
                    {
                        SequenceIndexStart = skipIntro ? -1 : 133,
                        SequenceIndexLoop = 134,
                        SequenceIndexEnd = 135,
                        StackIndex = _optionCount,
                        StackWidth = 0,
                        StackHeight = AnimatedSequenceDesc.DefaultStacking,
                        MessageId = MenuOptions[bitIndex],
                        MessageText = overrideTextField,
                        Flags = AnimationFlags.TextTranslateX,
                    }
                    : new AnimatedSequenceDesc
                    {
                        SequenceIndexLoop = 132,
                        SequenceIndexEnd = 135,
                        StackIndex = _optionCount,
                        StackWidth = 0,
                        StackHeight = AnimatedSequenceDesc.DefaultStacking,
                        MessageId = MenuOptions[bitIndex],
                        MessageText = overrideTextField,
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
                    };
                menuDesc.Add(desc);

                _optionCount++;
            }

            return SequenceFactory.Create(menuDesc);
        }

        public override void Open()
        {
            _backgroundSeq.Begin();
            _menuSeq.Begin();
            _characterSeq.Begin();
            base.Open();
        }

        public override void Close()
        {
            _backgroundSeq.End();
            _menuSeq.End();
            _characterSeq.End();
            base.Close();
        }

        protected override void MyUpdate(double deltaTime)
        {
            _backgroundSeq.Update(deltaTime);
            _menuSeq.Update(deltaTime);
            _characterSeq.Update(deltaTime);
        }

        protected override void MyDraw()
        {
            _backgroundSeq.Draw(0, 0);
            _menuSeq.Draw(0, 0);
            _characterSeq.Draw(0, 0);
        }
    }
}
