namespace OpenKh.Game.States.Title
{
    internal interface ITitleSubMenu
    {
        bool IsOpen { get; }

        void Invoke();
        void Update(double deltaTime);
        void Draw();
    }
}
