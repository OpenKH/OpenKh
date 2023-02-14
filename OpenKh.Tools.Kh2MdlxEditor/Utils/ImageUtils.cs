using OpenKh.Command.ImgTool.Utils;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MdlxEditor.Utils
{
    public class ImageUtils
    {
        public static Imgd pngToImgd(string filePath)
        {
            try
            {
                var inputFile = filePath;
                Imgd imgd;

                // Alpha enabled png → always 32 bpp
                using (var bitmap = new Bitmap(inputFile))
                {
                    imgd = ImgdBitmapUtil.ToImgd(bitmap, 8, null);

                    var buffer = new MemoryStream();
                    imgd.Write(buffer);
                }

                return imgd;
            }
            catch(Exception excep)
            {
                throw new Exception("Error loading texture: " + filePath);
            }
        }

        // OLD VERSION USING THE CONSOLE COMMAND VVV

        public static Imgd pngToImgd_Old(string filePath)
        {

            if (filePath.EndsWith(".imd"))
            {
                return ImageResizer.NormalizeImageSize(File.OpenRead(filePath).Using(s => Imgd.Read(s)));
            }

            if (!filePath.EndsWith(".png"))
            {
                throw new Exception("Not supported file to convert to Imgd");
            }
            if (!File.Exists(filePath))
            {
                throw new Exception("Texture doesn't exist: " + filePath);
            }

            var imdFile = Path.ChangeExtension(filePath, ".imd");

            if (File.Exists(imdFile))
            {
                Debug.WriteLine($"Skipping png to imd conversion, due to imd file existence and skipConversionIfExists option.");
            }
            else
            {
                var imgtoolOptions = "-b 8";
                Debug.WriteLine("OpenKh.Command.ImgTool.exe" + $"imd \"{filePath}\" -o \"{imdFile}\" {imgtoolOptions}");

                var result = new RunCmd(
                    "OpenKh.Command.ImgTool.exe",
                    $"imd \"{filePath}\" -o \"{imdFile}\" {imgtoolOptions}"
                );

                if (result.ExitCode != 0)
                {
                    throw new Exception($"ImgTool failed ({result.ExitCode})\n{result.StandardOutput}\n{result.StandardError}");
                }
            }

            Imgd imgdFile = pngToImgd(imdFile);

            File.Delete(imdFile);

            return imgdFile;
        }

        

        class RunCmd
        {
            private Process p;

            public string StandardOutput { get; }
            public string StandardError { get; }

            public string App => p.StartInfo.FileName;
            public int ExitCode => p.ExitCode;

            public RunCmd(string app, string arg)
            {
                var psi = new ProcessStartInfo(app, arg);
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                var p = Process.Start(psi);
                var stdOutAsync = Task.Run(() => p.StandardOutput.ReadToEnd());
                var stdErrAsync = Task.Run(() => p.StandardError.ReadToEnd());
                p.WaitForExit();
                this.p = p;
                StandardOutput = stdOutAsync.Result;
                StandardError = stdErrAsync.Result;
            }
        }
    }
}
