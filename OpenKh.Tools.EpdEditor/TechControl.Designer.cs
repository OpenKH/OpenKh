
namespace OpenKh.Tools.EpdEditor
{
    partial class TechControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TechParamGBox = new System.Windows.Forms.GroupBox();
            this.NumericSuccessRate = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.AttackAttribute = new System.Windows.Forms.ComboBox();
            this.AttackKind = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.TechniqueNumber = new System.Windows.Forms.TextBox();
            this.TechniquePower = new System.Windows.Forms.TextBox();
            this.TechParamGBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericSuccessRate)).BeginInit();
            this.SuspendLayout();
            // 
            // TechParamGBox
            // 
            this.TechParamGBox.Controls.Add(this.NumericSuccessRate);
            this.TechParamGBox.Controls.Add(this.label5);
            this.TechParamGBox.Controls.Add(this.label4);
            this.TechParamGBox.Controls.Add(this.label3);
            this.TechParamGBox.Controls.Add(this.AttackAttribute);
            this.TechParamGBox.Controls.Add(this.AttackKind);
            this.TechParamGBox.Controls.Add(this.label2);
            this.TechParamGBox.Controls.Add(this.label1);
            this.TechParamGBox.Controls.Add(this.TechniqueNumber);
            this.TechParamGBox.Controls.Add(this.TechniquePower);
            this.TechParamGBox.Location = new System.Drawing.Point(10, 0);
            this.TechParamGBox.Name = "TechParamGBox";
            this.TechParamGBox.Size = new System.Drawing.Size(446, 117);
            this.TechParamGBox.TabIndex = 0;
            this.TechParamGBox.TabStop = false;
            this.TechParamGBox.Text = "Parameters 1";
            // 
            // NumericSuccessRate
            // 
            this.NumericSuccessRate.Location = new System.Drawing.Point(269, 84);
            this.NumericSuccessRate.Maximum = new decimal(new int[] {
            65565,
            0,
            0,
            0});
            this.NumericSuccessRate.Name = "NumericSuccessRate";
            this.NumericSuccessRate.Size = new System.Drawing.Size(75, 23);
            this.NumericSuccessRate.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(270, 67);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(74, 15);
            this.label5.TabIndex = 5;
            this.label5.Text = "Success Rate";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(269, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(91, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "Attack Attribute";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "Attack Kind";
            // 
            // AttackAttribute
            // 
            this.AttackAttribute.FormattingEnabled = true;
            this.AttackAttribute.Items.AddRange(new object[] {
            "ATK_ATTR_NONE",
            "ATK_ATTR_PHYSICAL",
            "ATK_ATTR_FIRE",
            "ATK_ATTR_ICE",
            "ATK_ATTR_THUNDER",
            "ATK_ATTR_DARK",
            "ATK_ATTR_ZERO",
            "ATK_ATTR_SPECIAL",
            "ATK_ATTR_MAX"});
            this.AttackAttribute.Location = new System.Drawing.Point(269, 36);
            this.AttackAttribute.Name = "AttackAttribute";
            this.AttackAttribute.Size = new System.Drawing.Size(158, 23);
            this.AttackAttribute.TabIndex = 4;
            // 
            // AttackKind
            // 
            this.AttackKind.FormattingEnabled = true;
            this.AttackKind.Items.AddRange(new object[] {
            "ATK_KIND_NONE",
            "ATK_KIND_DMG_SMALL",
            "ATK_KIND_DMG_BIG",
            "ATK_KIND_DMG_BLOW",
            "ATK_KIND_DMG_TOSS",
            "ATK_KIND_DMG_BEAT",
            "ATK_KIND_DMG_FLICK",
            "ATK_KIND_POISON",
            "ATK_KIND_SLOW",
            "ATK_KIND_STOP",
            "ATK_KIND_BIND",
            "ATK_KIND_FAINT",
            "ATK_KIND_FREEZE",
            "ATK_KIND_BURN",
            "ATK_KIND_CONFUSE",
            "ATK_KIND_BLIND",
            "ATK_KIND_DEATH",
            "ATK_KIND_KILL",
            "ATK_KIND_CAPTURE",
            "ATK_KIND_MAGNET ",
            "ATK_KIND_ZEROGRAVITY",
            "ATK_KIND_AERO",
            "ATK_KIND_TORNADO",
            "ATK_KIND_DEGENERATOR",
            "ATK_KIND_WITHOUT",
            "ATK_KIND_EAT",
            "ATK_KIND_TREASURERAID",
            "ATK_KIND_SLEEPINGDEATH",
            "ATK_KIND_SLEEP",
            "ATK_KIND_MAGNET_MUNNY",
            "ATK_KIND_MAGNET_HP ",
            "ATK_KIND_MAGNET_FOCUS",
            "ATK_KIND_MINIMUM",
            "ATK_KIND_QUAKE",
            "ATK_KIND_RECOVER",
            "ATK_KIND_DISCOMMAND",
            "ATK_KIND_DISPRIZE_M",
            "ATK_KIND_DISPRIZE_H",
            "ATK_KIND_DISPRIZE_F",
            "ATK_KIND_DETONE",
            "ATK_KIND_GM_BLOW",
            "ATK_KIND_BLAST",
            "ATK_KIND_MAGNESPIRAL",
            "ATK_KIND_GLACIALARTS",
            "ATK_KIND_TRANSCENDENCE",
            "ATK_KIND_VENGEANCE",
            "ATK_KIND_MAGNEBREAKER",
            "ATK_KIND_MAGICIMPULSE_CF",
            "ATK_KIND_MAGICIMPULSE_CFB",
            "ATK_KIND_MAGICIMPULSE_CFBB",
            "ATK_KIND_DMG_RISE",
            "ATK_KIND_STUMBLE",
            "ATK_KIND_MOUNT",
            "ATK_KIND_IMPRISONMENT",
            "ATK_KIND_SLOWSTOP",
            "ATK_KIND_GATHERING",
            "ATK_KIND_EXHAUSTED"});
            this.AttackKind.Location = new System.Drawing.Point(6, 84);
            this.AttackKind.Name = "AttackKind";
            this.AttackKind.Size = new System.Drawing.Size(253, 23);
            this.AttackKind.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(124, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Technique Number";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Technique Power";
            // 
            // TechniqueNumber
            // 
            this.TechniqueNumber.Location = new System.Drawing.Point(124, 36);
            this.TechniqueNumber.Name = "TechniqueNumber";
            this.TechniqueNumber.Size = new System.Drawing.Size(100, 23);
            this.TechniqueNumber.TabIndex = 1;
            // 
            // TechniquePower
            // 
            this.TechniquePower.Location = new System.Drawing.Point(4, 36);
            this.TechniquePower.Name = "TechniquePower";
            this.TechniquePower.Size = new System.Drawing.Size(100, 23);
            this.TechniquePower.TabIndex = 0;
            // 
            // TechControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TechParamGBox);
            this.Name = "TechControl";
            this.Size = new System.Drawing.Size(467, 128);
            this.TechParamGBox.ResumeLayout(false);
            this.TechParamGBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericSuccessRate)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox TechParamGBox;
        public System.Windows.Forms.NumericUpDown NumericSuccessRate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.ComboBox AttackAttribute;
        public System.Windows.Forms.ComboBox AttackKind;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox TechniqueNumber;
        public System.Windows.Forms.TextBox TechniquePower;
    }
}
