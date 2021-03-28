namespace OpenKh.Game.Debugging
{
    public interface IDebugConsumer
    {
        public void DebugUpdate(IDebug debug);
        public void DebugDraw(IDebug debug);
    }
}
