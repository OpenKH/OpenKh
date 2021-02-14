using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Patcher
{
    public class PatcherProcessor
    {
        public void Patch(string originalAssets, string outputDir, string modFilePath)
        {
            var metadata = File.OpenRead(modFilePath).Using(Metadata.Read);
            var modBasePath = Path.GetDirectoryName(modFilePath);
            Patch(originalAssets, outputDir, metadata, modBasePath);
        }

        public void Patch(string originalAssets, string outputDir, Metadata metadata, string modBasePath)
        {
            try
            {
                if (metadata.Assets == null)
                    throw new Exception("No assets found.");

                PatchKh2(originalAssets, outputDir, modBasePath, metadata.Assets.Kh2);
            }
            catch (Exception ex)
            {
                throw new PatcherException(metadata, ex);
            }
        }

        private void PatchKh2(string originalAssets, string outputDir, string modPath, AssetKh2 assets)
        {
            if (assets == null)
                throw new Exception("No Kingdom Hearts II assets found.");
            if (assets.Binaries != null)
                PatchBinary(outputDir, modPath, assets.Binaries);
        }

        private void PatchBinary(string outputDir, string modPath, IEnumerable<AssetBinary> binaries)
        {
            foreach (var binaryEntry in binaries)
            {
                var srcFile = Path.Combine(modPath, binaryEntry.Name);
                var dstFile = Path.Combine(outputDir, binaryEntry.Name);
                var dstDir = Path.GetDirectoryName(dstFile);

                if (!File.Exists(srcFile))
                    throw new FileNotFoundException($"The mod does not contain the file {binaryEntry.Name}", srcFile);
                Directory.CreateDirectory(dstDir);
                File.Copy(srcFile, dstFile, true);
            }
        }
    }
}
