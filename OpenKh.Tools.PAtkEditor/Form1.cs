using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using OpenKh.Bbs;

namespace OpenKh.Tools.PAtkEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Stream patkFile;
        List<PAtk.PAtkData> DataList = new List<PAtk.PAtkData>();

        private void LoadPATK_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "PAtkData files (*.bin)|*.bin|All files (*.*)|*.*";
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if (patkFile != null)
                    patkFile.Close();
                patkFile = File.OpenRead(dialog.FileName);
                DataList = PAtk.Read(patkFile);

                int i = 1;
                foreach(PAtk.PAtkData dat in DataList)
                {
                    AttackDataList.Items.Add("Player Attack Entry " + i);
                    i++;
                }



                AttackDataList.SelectedIndex = 0;
                //SaveITCButton.Enabled = true;
            }
        }

        private void AttackDataList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = AttackDataList.SelectedIndex;
            PAtk.PAtkData patk = DataList[selectedIndex];

            NumericPlayEnd.Value = patk.frPlayEnd;
            NumericGroupEffect.Value = patk.EffectGroup;
            NumericFlag.Value = patk.Flag;
            NumericAnimation1.Value = patk.Animation1;
            NumericAnimation2.Value = patk.Animation2;
            NumericAnimation3.Value = patk.Animation3;
            NumericAnimation4.Value = patk.Animation4;
            NumericComboEnable.Value = patk.frComboEnable;
            NumericChangeEnable.Value = patk.frChangeEnable;
            NumericSEGroup.Value = patk.SEGroup;
            NumericGroupAttack1.Value = patk.GroupAttack1;
            NumericGroupAttack2.Value = patk.GroupAttack2;
            NumericGroupAttack3.Value = patk.GroupAttack3;
            NumericGroupAttack4.Value = patk.GroupAttack4;
            NumericTrigger1.Value = patk.frTrigger1;
            NumericTrigger2.Value = patk.frTrigger2;
            NumericTrigger3.Value = patk.frTrigger3;
            NumericTrigger4.Value = patk.frTrigger4;
            NumericBullet.Value = patk.Bullet;
            NumericCamera.Value = patk.Camera;
            NumericAttackPower.Value = patk.AttackPower;
            NumericAttackAttribute.Value = patk.AttackAttribute;
            NumericMarkStart.Value = patk.frMarkStart;
            NumericMarkEnd.Value = patk.frMarkEnd;
            NumericMoveStart.Value = patk.frMoveStart;
            NumericMoveEnd.Value = patk.frMoveEnd;
            NumericMaxDistance.Value = patk.MaximumDistance;
            NumericTranslation.Value = patk.Translation;
            NumericRange.Value = patk.Range;
            NumericSpeed.Value = patk.Speed;
            NumericRate.Value = patk.Rate;
            NumericExDash.Value = patk.ExDash;
            NumericExRise.Value = patk.ExRise;
        }
    }
}
