using OpenKh.Kh2;
using OpenKh.Tools.Common;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using Xe.Tools;

namespace OpenKh.Tools.BarEditor.Services
{
    public static class ToolsLoaderService
    {
        public static ToolInvokeDesc.ContentChangeInfo? OpenTool(string fileName, string temporaryFileName, Bar.Entry entry)
        {
            string name;

            switch (entry.Type)
            {
                // Disabling it, since it is veeeeeery buggy at the moment.
                //case Bar.EntryType.Bar:
                //	name = "OpenKh.Tools.BarEditor";
                //	break;
                case Bar.EntryType.Imgd:
                case Bar.EntryType.Imgz:
                    name = "OpenKh.Tools.ImageViewer";
                    break;
                case Bar.EntryType.Layout:
                    name = "OpenKh.Tools.LayoutViewer";
                    break;
                default:
                    throw new NotImplementedException($"Unable to find a tool for \"{entry.Type}\" files.");
            }

            var toolModule = Plugins
                .GetModules<IToolModule<ToolInvokeDesc>>(null, x => x.Contains(name) && Path.GetExtension(x) == ".dll")
                .FirstOrDefault();

            if (toolModule.Item1 == null || toolModule.Item2 == null)
            {
                MessageBox.Show($"Unable to find a tool module for the tool {name}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            var tool = Activator.CreateInstance(toolModule.Item2) as IToolModule<ToolInvokeDesc>;

            entry.Stream.Position = 0;
            var toolInvokeDesc = new ToolInvokeDesc
            {
                Title = $"{entry.Name}@{Path.GetFileName(fileName)}",
                ActualFileName = temporaryFileName,
                SelectedEntry = entry,
                ContentChange = ToolInvokeDesc.ContentChangeInfo.None
            };

            tool?.ShowDialog(toolInvokeDesc);

            return toolInvokeDesc.ContentChange;
        }

    }
}
