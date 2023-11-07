using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class SearchForKh2AssetFileUsecase
    {
        private readonly string _searchDirsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SearchDirs.txt");

        public string ResolveFilePath(string file)
        {
            if (File.Exists(file))
            {
                return file;
            }
            else
            {
                if (!File.Exists(_searchDirsFile))
                {
                    File.WriteAllText(_searchDirsFile, "");
                }

                foreach (var dir in File.ReadAllLines(_searchDirsFile)
                    .Where(dir => !string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
                )
                {
                    var path = Path.Combine(dir, file);
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }

                void Resolved(string newDir)
                {
                    var dirs = File.ReadAllLines(_searchDirsFile);
                    if (dirs.Contains(newDir))
                    {
                        return;
                    }
                    else
                    {
                        File.WriteAllLines(
                            _searchDirsFile,
                            dirs
                                .Append(newDir)
                        );
                    }
                }

                return Ask(file, Resolved) ?? file;
            }
        }

        private string? Ask(string file, Action<string> onNewDir)
        {
            string? selectedFile = null;

            string[] SplitToPathNodes(string path) =>
                path.Split(new char[] { '/', '\\' });

            FileDialog.OnOpen(
                loadFrom =>
                {
                    onNewDir(Path.GetDirectoryName(loadFrom)!);

                    var numRelativeDepth = SplitToPathNodes(file).Count();
                    if (2 <= numRelativeDepth)
                    {
                        onNewDir(
                            string.Join(
                                Path.DirectorySeparatorChar,
                                SplitToPathNodes(Path.GetDirectoryName(loadFrom)!)
                                    .Reverse()
                                    .Skip(numRelativeDepth - 1)
                                    .Reverse()
                            )
                        );
                    }

                    selectedFile = loadFrom;
                },
                FileDialogFilterComposer.Compose()
                    .AddAllFiles(),
                file
            );

            return selectedFile;
        }
    }
}
