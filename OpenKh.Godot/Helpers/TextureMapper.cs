using System.Collections.Generic;
using Godot;

namespace OpenKh.Godot.Helpers
{
    public class TextureMapper
    {
        public readonly List<ImageTexture> HdTextures = [];
        public readonly Dictionary<int, int> TextureMap = new();
        public int CurrentIndex { get; private set; }

        public TextureMapper(IEnumerable<ImageTexture> hdTextures, Dictionary<int, int> map = null, int index = 0)
        {
            if (hdTextures is not null) HdTextures.AddRange(hdTextures);
            if (map is not null) TextureMap = map;
            CurrentIndex = index;
        }

        public ImageTexture GetTexture(int index, ImageTexture fallback)
        {
            if (!TextureMap.TryGetValue(index, out var value))
            {
                value = CurrentIndex;
                TextureMap[index] = value;
                CurrentIndex++;
            }
            if (value >= HdTextures.Count) return fallback;
            
            return HdTextures[value] ?? fallback;
        }

        public ImageTexture GetNextTexture(ImageTexture fallback)
        {
            var value = CurrentIndex;
            CurrentIndex++;
            
            return HdTextures[value] ?? fallback;
        }
    }
}
