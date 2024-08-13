using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Godot;
using OpenKh.Egs;
using Environment = System.Environment;
using Helpers = OpenKh.Godot.addons.OpenKHImporter.Helpers;

namespace OpenKh.Godot.Test;

//TODO: i've separated this between extracted and imported for now, as the tools are incomplete and require rapid iteration
// if you try and import everything from any of the games your computer will cry

//TODO: move this somewhere else

//TODO: at what step in the pipeline should we be extracting assets? before godot is even started?
public partial class TestAssetExtractor : Node
{
    //Paths to original KH1 files to import
    //this will also search and find remastered files to copy
    private static readonly string[] ImportedKH1 =
    {
        "xa_ex_0010.mdls", //Sora
        //"xa_nm_0000.mdls", //Sora (Halloween Town)
		
    };
    private static readonly string[] ImportedKH2 =
    {
        "obj/P_EX100.mdlx", //Sora
        "obj/P_EX100.mset", //Sora's Moveset
    };
    
    public override void _Ready()
    {
        var gamePath = "/mnt/LocalDisk2/SteamLibrary/steamapps/common/KINGDOM HEARTS -HD 1.5+2.5 ReMIX-/Image/dt";
        var files = Directory.GetFiles(gamePath);
        foreach (var path in files)
        {
            if ((path.EndsWith(".hed") && Path.GetFileName(path)[3] == '_') || path.Contains("Recom.hed"))
            {
                _remainingPaths.Push(path);
            }
        }
        //EgsTools.
        Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Imported"));
        Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Extracted"));
        File.Create(Path.Combine(Environment.CurrentDirectory, "Extracted", ".gdignore"));
        //Console.SetOut(new ConsoleRedirect());
    }

    private ConcurrentStack<string> _remainingPaths = new();
    private ConcurrentStack<string> _messages = new();
    private Task _extractTask;
    private bool _imported = false;
    
    public override void _Process(double delta)
    {
        if ((_extractTask is null || _extractTask.IsCompleted) && _remainingPaths.TryPop(out var newPath))
        {
            var fileName = Path.GetFileName(newPath);
            var sub = fileName.Contains("Recom") ? "recom" : fileName[..3];
			
            var extractPath = Path.Combine(Environment.CurrentDirectory, "Extracted", sub);
			
            GD.Print($"{newPath} started");
			
            _extractTask = Task.Run(() =>
            {
                EgsTools.Extract(newPath, extractPath, true);
                _messages.Push($"{newPath} completed");
            });
        }
        else if (_extractTask is not null && _extractTask.IsCompleted && _remainingPaths.IsEmpty && !_imported)
        {
            _imported = true;

            Directory.CreateDirectory(Helpers.Kh1ImportOriginalPath);
            Directory.CreateDirectory(Helpers.Kh1ImportRemasteredPath);
			
            Directory.CreateDirectory(Helpers.Kh2ImportOriginalPath);
            Directory.CreateDirectory(Helpers.Kh2ImportRemasteredPath);
			
            foreach (var item in ImportedKH1)
            {
                GD.Print(item);
                var originalDirectory = Path.Combine(Helpers.Kh1ImportOriginalPath, item);
                Directory.CreateDirectory(Path.GetDirectoryName(originalDirectory));
                File.Copy(Path.Combine(Helpers.Kh1OriginalPath, item), Path.Combine(Helpers.Kh1ImportOriginalPath, item), true);

                if (!Directory.Exists(Path.Combine(Helpers.Kh1RemasteredPath, item))) continue;
				
                var remasteredDirectory = Path.Combine(Helpers.Kh1ImportRemasteredPath, item);
                Directory.CreateDirectory(remasteredDirectory);
                CopyFilesRecursively(Path.Combine(Helpers.Kh1RemasteredPath, item), remasteredDirectory);
            }
            foreach (var item in ImportedKH2)
            {
                GD.Print(item);
                var originalDirectory = Path.Combine(Helpers.Kh2ImportOriginalPath, item);
                Directory.CreateDirectory(Path.GetDirectoryName(originalDirectory));
                File.Copy(Path.Combine(Helpers.Kh2OriginalPath, item), Path.Combine(Helpers.Kh2ImportOriginalPath, item), true);

                if (!Directory.Exists(Path.Combine(Helpers.Kh2RemasteredPath, item))) continue;
				
                var remasteredDirectory = Path.Combine(Helpers.Kh2ImportRemasteredPath, item);
                Directory.CreateDirectory(remasteredDirectory);
                CopyFilesRecursively(Path.Combine(Helpers.Kh2RemasteredPath, item), remasteredDirectory);
            }
        }
        while (_messages.TryPop(out var msg)) GD.Print(msg);
        //while (ConsoleRedirect.MsgStack.TryPop(out var consoleMsg)) GD.Print(consoleMsg);
    }

    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)) Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)) File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
    }
}
