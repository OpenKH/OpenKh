using OpenKh.Bbs;
using OpenKh.Imaging;

namespace OpenKh.Tools.CtdEditor.ViewModels
{
    public class FontEntryViewModel
    {
        private readonly FontsArc.Font _font;

        public string Name => _font.Name;
        public IImageRead Font1 => _font.Image1;
        public IImageRead Font2 => _font.Image2;
        public FontInfo Info => _font.Info;
        public FontCharacterInfo[] CharactersInfo => _font.CharactersInfo;

        public FontEntryViewModel(FontsArc.Font font)
        {
            _font = font;
        }
    }
}
