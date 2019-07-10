using OpenKh.Kh2;
using OpenKh.Tools.Common;
using System;
using System.IO;
using System.Linq;
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
					name = "OpenKh.Tools.ImgdViewer";
					break;
				case Bar.EntryType.Imgz:
					name = "OpenKh.Tools.ImgzViewer";
					break;
				default:
					throw new NotImplementedException($"Unable to find a tool for \"{type}\" files.");
			}

			var toolModule = Plugins
				.GetModules<IToolModule<ToolInvokeDesc>>(null, x => x.Contains(name) && Path.GetExtension(x) == ".exe")
				.FirstOrDefault();

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
