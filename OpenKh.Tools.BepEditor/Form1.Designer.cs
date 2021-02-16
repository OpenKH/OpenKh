
namespace OpenKh.Tools.BepEditor
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BaseStatsLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.LoadBEP = new System.Windows.Forms.Button();
            this.SaveBEP = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.DisappearLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.BaseStatsLayout);
            this.groupBox1.Location = new System.Drawing.Point(12, 46);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(410, 298);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Base Stats";
            // 
            // BaseStatsLayout
            // 
            this.BaseStatsLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BaseStatsLayout.AutoScroll = true;
            this.BaseStatsLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.BaseStatsLayout.Location = new System.Drawing.Point(7, 22);
            this.BaseStatsLayout.Name = "BaseStatsLayout";
            this.BaseStatsLayout.Size = new System.Drawing.Size(394, 263);
            this.BaseStatsLayout.TabIndex = 0;
            this.BaseStatsLayout.WrapContents = false;
            // 
            // LoadBEP
            // 
            this.LoadBEP.Location = new System.Drawing.Point(19, 13);
            this.LoadBEP.Name = "LoadBEP";
            this.LoadBEP.Size = new System.Drawing.Size(75, 23);
            this.LoadBEP.TabIndex = 1;
            this.LoadBEP.Text = "Load BEP";
            this.LoadBEP.UseVisualStyleBackColor = true;
            this.LoadBEP.Click += new System.EventHandler(this.LoadBEP_Click);
            // 
            // SaveBEP
            // 
            this.SaveBEP.Enabled = false;
            this.SaveBEP.Location = new System.Drawing.Point(100, 12);
            this.SaveBEP.Name = "SaveBEP";
            this.SaveBEP.Size = new System.Drawing.Size(75, 23);
            this.SaveBEP.TabIndex = 2;
            this.SaveBEP.Text = "Save as...";
            this.SaveBEP.UseVisualStyleBackColor = true;
            this.SaveBEP.Click += new System.EventHandler(this.SaveBEP_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.DisappearLayout);
            this.groupBox2.Location = new System.Drawing.Point(13, 351);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(409, 232);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Disappear Parameters";
            // 
            // DisappearLayout
            // 
            this.DisappearLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DisappearLayout.AutoScroll = true;
            this.DisappearLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.DisappearLayout.Location = new System.Drawing.Point(7, 23);
            this.DisappearLayout.Name = "DisappearLayout";
            this.DisappearLayout.Size = new System.Drawing.Size(393, 203);
            this.DisappearLayout.TabIndex = 0;
            this.DisappearLayout.WrapContents = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(436, 591);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.SaveBEP);
            this.Controls.Add(this.LoadBEP);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "BEP Editor";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.FlowLayoutPanel BaseStatsLayout;
        private System.Windows.Forms.Button LoadBEP;
        private System.Windows.Forms.Button SaveBEP;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.FlowLayoutPanel DisappearLayout;
    }
}

