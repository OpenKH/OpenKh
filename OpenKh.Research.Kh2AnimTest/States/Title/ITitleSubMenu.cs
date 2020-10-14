namespace OpenKh.Research.Kh2AnimTest.States.Title
{
    internal interface ITitleSubMenu
    {
        bool IsOpen { get; }

        void Invoke();
        void Update(double deltaTime);
        void Draw();
    }
}
