using System.Numerics;

namespace OpenKh.Godot.Animation
{
    public struct KH2Bone
    {
        public int Sibling;
        public int Parent;
        public int Child;
        public int Flags;
        //these are intentionally using systems.numerics types
        //i want to have the ANB playback be framework agnostic
        public Vector4 RestScale;
        public Vector4 RestRotation;
        public Vector4 RestPosition;
    }
}
