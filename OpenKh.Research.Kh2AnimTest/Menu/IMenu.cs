namespace OpenKh.Research.Kh2AnimTest.Menu
{
    public interface IMenu
    {
        ushort MenuNameId { get; }
        bool IsClosed { get; }

        void Open();
        void Close();
        void Push(IMenu subMenu);

        void Update(double deltaTime);
        void Draw();
    }
}
