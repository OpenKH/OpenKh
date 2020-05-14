using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Extensions;
using OpenKh.Imaging;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Game
{
    public static class Extensions
    {
        public static void ForEntry(this IEnumerable<Bar.Entry> entries, string name, Action<Stream> action)
        {
            var entry = entries.FirstOrDefault(x => x.Name == name);
            if (entry != null)
                action(entry.Stream);
        }

        public static T ForEntry<T>(this IEnumerable<Bar.Entry> entries, string name, Func<Stream, T> action)
        {
            var entry = entries.FirstOrDefault(x => x.Name == name);
            if (entry != null)
                return action(entry.Stream);

            return default;
        }

        public static void ForEntry(this IEnumerable<Bar.Entry> entries, string name, Bar.EntryType type, Action<Stream> action)
        {
            var entry = entries.FirstOrDefault(x => x.Name == name);
            if (entry != null)
                action(entry.Stream);
        }

        public static T ForEntry<T>(this IEnumerable<Bar.Entry> entries, string name, Bar.EntryType type, Func<Stream, T> action)
        {
            var entry = entries.FirstOrDefault(x => x.Name == name);
            if (entry != null)
                return action(entry.Stream);

            return default;
        }

        public static Texture2D CreateTexture(this IImageRead image, GraphicsDevice graphicsDevice)
        {
            var size = image.Size;
            var texture = new Texture2D(graphicsDevice, size.Width, size.Height);

            texture.SetData(image.AsRgba8888());

            return texture;
        }
    }
}
