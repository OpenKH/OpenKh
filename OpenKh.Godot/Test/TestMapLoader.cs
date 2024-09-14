using System.IO;
using System.Linq;
using Godot;
using OpenKh.Godot.Conversion;
using OpenKh.Godot.Helpers;
using OpenKh.Kh2;
using FileAccess = Godot.FileAccess;

namespace OpenKh.Godot.Test
{
    [Tool]
    public partial class TestMapLoader : Node3D
    {
        [Export] private bool done = false;
        public override void _Process(double delta)
        {
            if (done) return;
            
            GD.Print("doing");

            done = true;
            
            var sourceFile = "res://Imported/kh2/original/map/hb26.map";

            var realPath = ProjectSettings.GlobalizePath(sourceFile);

            var name = Path.GetFileNameWithoutExtension(realPath);

            var relativePath = Path.GetRelativePath(ImportHelpers.Kh2ImportOriginalPath, realPath);

            var hdTexturesPath = Path.Combine(ImportHelpers.Kh2ImportRemasteredPath, relativePath);

            var usesHdTextures = false;
            var hdTextures = new System.Collections.Generic.Dictionary<int, string>();

            if (Directory.Exists(hdTexturesPath))
            {
                usesHdTextures = true;
                foreach (var filePath in Directory.GetFiles(hdTexturesPath))
                {
                    var textureName = Path.GetFileName(filePath);

                    if (!textureName.StartsWith('-') || !int.TryParse(textureName[1..^4], out var number)) continue;

                    var localPath = ProjectSettings.LocalizePath(filePath);
                    hdTextures[number] = localPath;
                }
            }

            using var stream = File.Open(realPath, FileMode.Open, System.IO.FileAccess.Read);

            stream.Seek(0, SeekOrigin.Begin);

            if (!Bar.IsValid(stream)) return;

            var barFile = Bar.Read(stream);

            Converters.FromMap(barFile);

            var images = usesHdTextures ? Enumerable.Repeat((ImageTexture)null, hdTextures.Max(i => i.Key) + 1).ToList() : [];

            if (usesHdTextures)
            {
                foreach (var index in hdTextures)
                {
                    try
                    {
                        var texture = ResourceLoader.Load<ImageTexture>(index.Value);
                        images[index.Key] = texture;
                    }
                    catch
                    {
                        images[index.Key] = null;
                        // ignored
                    }
                }
            }

            var result = Converters.FromMap(barFile, images);
            
            AddChild(result);
        }
    }
}
