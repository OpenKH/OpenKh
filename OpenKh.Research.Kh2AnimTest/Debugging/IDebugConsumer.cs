namespace OpenKh.Research.Kh2AnimTest.Debugging
{
    public interface IDebugConsumer
    {
        public void DebugUpdate(IDebug debug);
        public void DebugDraw(IDebug debug);
    }
}
