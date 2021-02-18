namespace OpenKh.Engine.Input
{
    public interface IInputButtons
    {
        bool Up { get; }
        bool Down { get; }
        bool Left { get; }
        bool Right { get; }
        bool Cross { get; }
        bool Circle { get; }
        bool Square { get; }
        bool Triangle { get; }
        bool Select { get; }
        bool Start { get; }
        bool L1 { get; }
        bool L2 { get; }
        bool L3 { get; }
        bool R1 { get; }
        bool R2 { get; }
        bool R3 { get; }
        bool Confirm { get; }
        bool Cancel { get; }
    }
}
