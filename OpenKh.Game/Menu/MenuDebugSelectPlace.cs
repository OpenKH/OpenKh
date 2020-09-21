using OpenKh.Engine.Renderers;
using OpenKh.Game.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.Menu
{
    public class MenuDebugSelectPlace : MenuBase
    {
        private class PlaceDescriptor
        {
            public int WorldIndex { get; set; }
            public int PlaceIndex { get; set; }
            public ushort MessageId { get; set; }
        }

        const int MaxOptionPerColumn = 10;
        public static string Name = "SELECT PLACE";
        private readonly PlaceDescriptor[] _places;
        private IAnimatedSequence _menuSeq;
        private int _selectedOption;

        public int SelectedOption
        {
            get => _selectedOption;
            set
            {
                value = value < 0 ? _places.Length - 1 : value % _places.Length;
                if (_selectedOption != value)
                {
                    _selectedOption = value;
                    _menuSeq = InitializeMenu(true);
                    _menuSeq.Begin();
                }
            }
        }

        public override ushort MenuNameId { get; }
        protected override bool IsEnd => _menuSeq.IsEnd;

        public MenuDebugSelectPlace(
            IMenuManager menuManager,
            int worldId,
            ushort worldMessageId) : base(menuManager)
        {
            MenuNameId = worldMessageId;

            var worldKey = Kh2.Constants.WorldIds[worldId];
            _places = menuManager.GameContext.Kernel.Places[worldKey]
                .Select((x, Index) => new { x.MessageId, Index })
                .Where(x => menuManager.GameContext.Kernel.DataContent
                    .FileExists(menuManager.GameContext.Kernel.GetMapFileName(worldId, x.Index)))
                .Select(x => new PlaceDescriptor
                {
                    WorldIndex = worldId,
                    PlaceIndex = x.Index,
                    MessageId = x.MessageId,
                })
                .ToArray();

            _menuSeq = InitializeMenu(false);
        }

        private IAnimatedSequence InitializeMenu(bool skipIntro)
        {
            return SequenceFactory.Create(Enumerable.Range(0, _places.Length)
                .Select(i => new AnimatedSequenceDesc
                {
                    X = i / MaxOptionPerColumn * 140,
                    SequenceIndexStart = skipIntro ? -1 : 133,
                    SequenceIndexLoop = SelectedOption == i ? 132 : 134,
                    SequenceIndexEnd = 135,
                    StackIndex = i % MaxOptionPerColumn,
                    StackWidth = 0,
                    StackHeight = AnimatedSequenceDesc.DefaultStacking,
                    Flags = AnimationFlags.TextTranslateX |
                        AnimationFlags.ChildStackHorizontally,
                    MessageId = _places[i].MessageId,
                    Children = SelectedOption == i ? new List<AnimatedSequenceDesc>
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
                    } : new List<AnimatedSequenceDesc>()
                }));
        }

        protected override void ProcessInput(InputManager inputManager)
        {
            if (inputManager.IsMenuUp)
                SelectedOption--;
            else if (inputManager.IsMenuDown)
                SelectedOption++;
            if (inputManager.IsMenuLeft)
                SelectedOption -= MaxOptionPerColumn;
            if (inputManager.IsMenuRight)
                SelectedOption += MaxOptionPerColumn;
            else if (inputManager.IsCircle)
            {
                var place = _places[SelectedOption];
                MenuManager.GameContext.LoadPlace(
                    place.WorldIndex,
                    place.PlaceIndex,
                    0);

                MenuManager.CloseAllMenu();
            }
            else
                base.ProcessInput(inputManager);
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

        public override string ToString() => Name;
    }
}
