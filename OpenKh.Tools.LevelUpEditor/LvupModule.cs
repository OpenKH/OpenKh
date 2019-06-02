using Xe.Tools;

namespace OpenKh.Tools.LevelUpEditor
{
    public class LvupModule : IToolModule
    {
        public bool? ShowDialog(params object[] args)
        {
            return new LvupView(args).ShowDialog();
        }
    }
}
