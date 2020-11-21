namespace OpenKh.Engine.Motion
{
    public interface IMotionEngine
    {
        void ApplyMotion(IModelMotion model, float time);
    }
}
