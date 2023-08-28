using OpenKh.Kh2;
using System.IO;

namespace OpenKh.Tools.Kh2ObjectEditor.Services
{
    public class S_Clipboard
    {
        private Stream Copy_Dpd { get; set; }

        public void copyDpd(Dpd dpd)
        {
            Copy_Dpd = dpd.getAsStream();
        }
        public Dpd pasteDpd()
        {
            if (Copy_Dpd == null)
                return null;

            return new Dpd(Copy_Dpd);
        }


        // SINGLETON
        private S_Clipboard() { }
        private static S_Clipboard instance = null;
        public static S_Clipboard Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new S_Clipboard();
                }
                return instance;
            }
        }
        public static void reset()
        {
            instance = new S_Clipboard();
        }
    }
}
