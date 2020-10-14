﻿using OpenKh.Research.Kh2AnimTest.Debugging;
using OpenKh.Research.Kh2AnimTest.Infrastructure;
using System.IO;

namespace OpenKh.Research.Kh2AnimTest.DataContent
{
    public class ModDataContent : IDataContent
    {
        public bool FileExists(string fileName) => File.Exists(GetPath(fileName));

        public Stream FileOpen(string path)
        {
            var fileName = GetPath(path);
            if (File.Exists(fileName))
            {
                Log.Info($"Load mod {path}");
                return File.OpenRead(fileName);
            }

            return null;
        }

        private string GetPath(string path) => Path.Combine(Config.ModPath, path);
    }
}
