namespace OpenKh.Game.Menu
{
    interface IMenu
    {
        int SelectedOption { get; }

        void Open();
        void Close();

        void Update(double deltaTime);
        void Draw();
    }
}
