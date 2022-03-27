using OpenKh.Kh2.TextureFooter;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    internal class TexAnim_VM
    {
        public TextureAnimation texAnim { get; set; }
        public BitmapSource bitmapImage { get; set; }
        public ObservableCollection<TextureFrameGroupWrapper> TexFrameGroupList { get; set; }
        public ObservableCollection<TextureFrame> TextureFrameList { get; set; }

        public TexAnim_VM(TextureAnimation texAnim, byte[] clutPalette)
        {
            this.texAnim = texAnim;
            if(clutPalette != null) {
                bitmapImage = GetBimapSource(ToBgra32(texAnim.SpriteImage, clutPalette), texAnim.SpriteWidth, texAnim.SpriteHeight * texAnim.NumSpritesInImageData);
            }

            TexFrameGroupList = new ObservableCollection<TextureFrameGroupWrapper>();
            for(int i = 0; i < texAnim.FrameGroupList.Length; i++)
            {
                TexFrameGroupList.Add(new TextureFrameGroupWrapper(texAnim.FrameGroupList[i], i.ToString()));
            }

            TextureFrameList = new ObservableCollection<TextureFrame>();
            if(texAnim.FrameGroupList.Length > 0)
            {
                TextureFrameList = new ObservableCollection<TextureFrame>(texAnim.FrameGroupList[0].IndexedFrameList.Values);
            }
        }

        // Makes the Indexed8 Image a BGRA32 Image
        public static byte[] ToBgra32(byte[] image, byte[] clutPalette)
        {
            return OpenKh.Imaging.ImageDataHelpers.FromIndexed8ToBitmap32(image, clutPalette, OpenKh.Imaging.ImageDataHelpers.RGBA);
        }
        // Gets the bitmapSource from a BGRA32 Image
        // NOTE: Can be done using pixelformat Indexed8 to create a BitmapSource but it needs a palette instead of null
        public static BitmapSource GetBimapSource(byte[] imageBGRA32, int width, int height)
        {
            const double dpi = 96.0;

            return BitmapSource.Create(width, height, dpi, dpi, System.Windows.Media.PixelFormats.Bgra32, null, imageBGRA32, width * 4);
        }

        public void loadTextureFrameList(TextureFrameGroupWrapper texFrameGroupWrapper)
        {
            TextureFrameList.Clear();
            foreach (TextureFrame texFrame in texFrameGroupWrapper.texFrameGroup.IndexedFrameList.Values)
            {
                TextureFrameList.Add(texFrame);
            }
        }

        public class TextureFrameGroupWrapper
        {
            public string groupName { get; set; }
            public TextureFrameGroup texFrameGroup { get; set; }

            public TextureFrameGroupWrapper(TextureFrameGroup texFrameGroup, string identifier)
            {
                this.texFrameGroup = texFrameGroup;
                groupName = "Animation " + identifier;
            }
        }
    }
}
