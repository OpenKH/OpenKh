using OpenKh.Kh2;
using System.IO;
using System.Linq;

namespace OpenKh.Tools.Kh2MsetEditor.ViewModels
{
    public class Motion_VM
    {
        public Motion.InterpolatedMotion MotionFile { get; set; }
        public float TimeMultiplier { get; set; }
        public float StartFrom { get; set; }

        public Motion_VM() { }
        public Motion_VM(Motion.InterpolatedMotion motion)
        {
            MotionFile = motion;
            TimeMultiplier = 1.0f;
        }

        public void multiplyTimes()
        {
            foreach (int i in Enumerable.Range(0, MotionFile.KeyTimes.Count)) {
                MotionFile.KeyTimes[i] *= TimeMultiplier;
            }
        }
        public void startFrom()
        {
            foreach (int i in Enumerable.Range(0, MotionFile.KeyTimes.Count))
            {
                if(MotionFile.KeyTimes[i] < StartFrom) {
                    MotionFile.KeyTimes[i] = 0;
                }
                else {
                    MotionFile.KeyTimes[i] -= StartFrom;
                }
            }
        }

        public void exportAsJson()
        {
            if (MotionFile == null)
                return;

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Save motion as json";
            sfd.FileName = "motion.json";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                using (StreamWriter outputFile = new StreamWriter(sfd.FileName))
                {
                    outputFile.WriteLine(System.Text.Json.JsonSerializer.Serialize(MotionFile));
                }
            }
        }
        public void exportAsMotion()
        {
            if (MotionFile == null)
                return;

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Save motion as anb";
            sfd.FileName = "motion.motion";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                MemoryStream memStream = new MemoryStream();
                MotionFile.toStream().CopyTo(memStream);
                File.WriteAllBytes(sfd.FileName, memStream.ToArray());
            }
        }
    }
}
