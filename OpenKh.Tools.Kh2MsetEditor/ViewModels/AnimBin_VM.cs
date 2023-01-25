using OpenKh.Kh2;
using System.IO;

namespace OpenKh.Tools.Kh2MsetEditor.ViewModels
{
    public class AnimBin_VM
    {
        public AnimationBinary AnimationBinaryFile { get; set; }
        public AnimBin_VM() { }
        public AnimBin_VM(Stream stream)
        {
            loadNewAnimationBinary(stream);
        }
        public void loadNewAnimationBinary(Stream stream)
        {
            if (!Bar.IsValid(stream))
                return;

            AnimationBinaryFile = new AnimationBinary(stream);
        }
    }
}
