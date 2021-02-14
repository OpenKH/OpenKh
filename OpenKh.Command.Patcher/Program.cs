using OpenKh.Patcher;

namespace OpenKh.Command.Patcher
{
    class Program
    {
        static void Main(string[] args)
        {
            const string SourceAssets = "export_fm";
            const string OutputMod = "mod";
            const string MyMod = "mymod";

            var patcherProcessor = new PatcherProcessor();
            patcherProcessor.Patch(SourceAssets, OutputMod, MyMod + "/mod.yml");
        }
    }
}
