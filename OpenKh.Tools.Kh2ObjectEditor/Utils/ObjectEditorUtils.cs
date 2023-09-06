using Simple3DViewport.Objects;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OpenKh.Tools.Kh2ObjectEditor.Utils
{
    public class ObjectEditorUtils
    {
        public static bool isFilePathValid(string filepath, string extension)
        {
            if (!File.Exists(filepath))
                return false;
            if (!filepath.ToLower().EndsWith("."+extension))
                return false;

            return true;
        }

        public static bool isFilePathValid(string filepath, List<string> validExtensions)
        {
            if (!File.Exists(filepath))
                return false;

            bool validExtension = false;
            foreach(string extension in validExtensions)
            {
                if (filepath.ToLower().EndsWith("." + extension))
                {
                    validExtension = true;
                    break;
                }
            }

            if (!validExtension)
                return false;

            return true;
        }

        public static SimpleModel getBones(List<Vector3D> boneList)
        {
            SimpleModel model = new SimpleModel();
            foreach(Vector3D bonePos in boneList)
            {
                model.Meshes.Add(new SimpleMesh(Simple3DViewport.Utils.GeometryShapes.getCube(1, bonePos, Color.FromRgb(255, 255, 0))));
            }

            return model;
        }
    }
}
