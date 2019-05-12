using Xe.Tools;

namespace kh.tools.lvup
{
    public class LvupModule : IToolModule
    {
        public bool? ShowDialog(params object[] args)
        {
            return new LvupView(args).ShowDialog();
        }
    }
}
