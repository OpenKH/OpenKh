namespace OpenKh.Command.AnbMaker.Utils.JsonAnimSource
{
    public class BAnimationAction
    {
        public string Name { get; set; }
        public float FrameStart { get; set; }
        public float FrameEnd { get; set; }
        public BActionGroup[] Groups { get; set; }
    }
}
