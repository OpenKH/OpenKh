namespace OpenKh.Game.States.Title
{
    internal class TitleLayout
    {
        public int FadeIn { get; set; }
        public int Copyright { get; set; }
        public int Intro { get; set; }
        public int IntroSkip { get; set; }
        public int NewGame { get; set; }
        public int MenuOptionNewGame { get; set; }
        public int MenuOptionLoad { get; set; }
        public int MenuOptionTheater { get; set; }
        public int MenuOptionBack { get; set; }
        public bool HasTheater { get; set; }
        public bool HasBack { get; set; }
    }
}
