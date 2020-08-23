using OpenKh.Engine.Renderers;
using OpenKh.Game.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.Menu
{
    public class MainMenu : IMenu
    {
        private const int MaxCharacterCount = 4;
        private const int MaxMenuElementCount = 8;
        private const int MenuOptionSelectedSeq = 132;
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

        private readonly AnimatedSequenceFactory _animSeqFactory;
        private readonly InputManager _inputManager;
        private bool _isClosing;
        private IMenu _subMenu;
        private IAnimatedSequence _menuSeq;
        private IAnimatedSequence _characterSeq;

        private int _optionCount = 0;
        private int _optionSelected = 0;

        public bool IsClosed { get; private set; }
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

                var frame = _menuSeq.FrameIndex;
                _menuSeq = InitializeMenuOptions(true);
                _menuSeq.Begin();
                _menuSeq.FrameIndex = frame;
            }
        }

        public MainMenu(
            AnimatedSequenceFactory animatedSequenceFactory,
            InputManager inputManager)
        {
            _animSeqFactory = animatedSequenceFactory;
            _inputManager = inputManager;
            InitializeMenu();
        }

        private void InitializeMenu()
        {
            _menuSeq = InitializeMenuOptions();

            _characterSeq = _animSeqFactory.Create(Enumerable.Range(0, 5)
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

        private void ProcessInput(InputManager inputManager)
        {
            if (_isClosing)
                return;

            if (inputManager.IsMenuUp)
                SelectedOption--;
            if (inputManager.IsMenuDown)
                SelectedOption++;
            if (inputManager.IsCircle)
            {
                Push(new MenuTemplate(_animSeqFactory, _inputManager, 0));
            }
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

                AnimatedSequenceDesc desc = _optionCount != _optionSelected
                    ? new AnimatedSequenceDesc
                    {
                        SequenceIndexStart = skipIntro ? -1 : 133,
                        SequenceIndexLoop = 134,
                        SequenceIndexEnd = 135,
                        StackIndex = _optionCount,
                        StackWidth = 0,
                        StackHeight = AnimatedSequenceDesc.DefaultStacking,
                        MessageId = MenuOptions[bitIndex]
                    }
                    : new AnimatedSequenceDesc
                    {
                        SequenceIndexLoop = MenuOptionSelectedSeq,
                        StackIndex = _optionCount,
                        StackWidth = 0,
                        StackHeight = AnimatedSequenceDesc.DefaultStacking,
                        MessageId = MenuOptions[bitIndex],
                        Children = new List<AnimatedSequenceDesc>
                        {
                            new AnimatedSequenceDesc
                            {
                                SequenceIndexLoop = 25,
                                TextAnchor = TextAnchor.BottomLeft,
                            },
                            new AnimatedSequenceDesc
                            {
                                SequenceIndexStart = skipIntro ? -1 : 27,
                                SequenceIndexLoop = 28,
                                SequenceIndexEnd = 29,
                                TextAnchor = TextAnchor.BottomRight,
                                StackIndex = 1,
                            }
                        }
                    };
                menuDesc.Add(desc);

                _optionCount++;
            }

            return _animSeqFactory.Create(menuDesc);
        }

        public void Open()
        {
            _isClosing = false;
            _menuSeq.Begin();
            _characterSeq.Begin();
        }

        public void Close()
        {
            _isClosing = true;
            _menuSeq.End();
            _characterSeq.End();
        }

        public void Push(IMenu menu)
        {
            _subMenu = menu;
            _subMenu.Open();
        }

        public void Update(double deltaTime)
        {
            if (_subMenu == null)
            {
                _menuSeq.Update(deltaTime);
                _characterSeq.Update(deltaTime);
                ProcessInput(_inputManager);
            }
            else
            {
                _subMenu.Update(deltaTime);
                if (_subMenu.IsClosed)
                {
                    _subMenu = null;
                    Open();
                }
            }

        }

        public void Draw()
        {
            if (_subMenu == null)
            {
                _menuSeq.Draw(0, 0);
                _characterSeq.Draw(0, 0);
            }
            else
                _subMenu.Draw();
        }
    }
}
