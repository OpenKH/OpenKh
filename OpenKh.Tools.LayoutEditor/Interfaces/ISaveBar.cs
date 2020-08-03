using OpenKh.Kh2;

namespace OpenKh.Tools.LayoutEditor.Interfaces
{
    public interface ISaveBar
    {
        Bar.Entry SaveAnimation(string name);
        Bar.Entry SaveTexture(string name);
    }
}
