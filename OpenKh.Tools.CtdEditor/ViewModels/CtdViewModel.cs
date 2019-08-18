using OpenKh.Bbs;

namespace OpenKh.Tools.CtdEditor.ViewModels
{
    public class CtdViewModel
    {
        public Ctd Ctd { get; private set; }

        public CtdViewModel() :
            this(new Ctd())
        {

        }

        public CtdViewModel(Ctd ctd)
        {
            Ctd = ctd;
        }
    }
}
