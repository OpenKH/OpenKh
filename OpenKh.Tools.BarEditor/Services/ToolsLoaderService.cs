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
		public static bool? OpenTool(string fileName, Bar.Entry entry)
		{
			string name;

			switch (entry.Type)
			{
				case Bar.EntryType.Bar:
					name = "OpenKh.Tools.BarEditor";
					break;
				case Bar.EntryType.Imgd:
				case Bar.EntryType.Imgz:
					name = "OpenKh.Tools.ImageViewer";
					break;
				default:
					throw new NotImplementedException($"Unable to find a tool for \"{entry.Type}\" files.");
			}

			var toolModule = Plugins
				.GetModules<IToolModule<ToolInvokeDesc>>(null, x => x.Contains(name) && Path.GetExtension(x) == ".exe")
				.FirstOrDefault();

            if (toolModule.Item1 == null || toolModule.Item2 == null)
            {
                MessageBox.Show($"Unable to find a tool module for the tool {name}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

			var tool = Activator.CreateInstance(toolModule.Item2) as IToolModule<ToolInvokeDesc>;

			entry.Stream.Position = 0;
			return tool?.ShowDialog(new ToolInvokeDesc
            {
                FileName = fileName,
                SelectedEntry = entry
            });
		}

	}
}
