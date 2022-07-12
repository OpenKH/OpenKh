
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
            this.BaseStatsLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.LoadBEP = new System.Windows.Forms.Button();
            this.SaveBEP = new System.Windows.Forms.Button();
            this.DisappearLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.TabBaseStats = new System.Windows.Forms.TabPage();
            this.TabDisappearParameters = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.TabBaseStats.SuspendLayout();
            this.TabDisappearParameters.SuspendLayout();
            this.SuspendLayout();
            // 
            // BaseStatsLayout
            // 
            this.BaseStatsLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BaseStatsLayout.AutoScroll = true;
            this.BaseStatsLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.BaseStatsLayout.Location = new System.Drawing.Point(3, 3);
            this.BaseStatsLayout.Name = "BaseStatsLayout";
            this.BaseStatsLayout.Size = new System.Drawing.Size(443, 663);
            this.BaseStatsLayout.TabIndex = 0;
            this.BaseStatsLayout.WrapContents = false;
            // 
            // LoadBEP
            // 
            this.LoadBEP.Location = new System.Drawing.Point(13, 12);
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
            this.SaveBEP.Location = new System.Drawing.Point(94, 11);
            this.SaveBEP.Name = "SaveBEP";
            this.SaveBEP.Size = new System.Drawing.Size(75, 23);
            this.SaveBEP.TabIndex = 2;
            this.SaveBEP.Text = "Save as...";
            this.SaveBEP.UseVisualStyleBackColor = true;
            this.SaveBEP.Click += new System.EventHandler(this.SaveBEP_Click);
            // 
            // DisappearLayout
            // 
            this.DisappearLayout.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DisappearLayout.AutoScroll = true;
            this.DisappearLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.DisappearLayout.Location = new System.Drawing.Point(3, 3);
            this.DisappearLayout.Name = "DisappearLayout";
            this.DisappearLayout.Size = new System.Drawing.Size(457, 382);
            this.DisappearLayout.TabIndex = 0;
            this.DisappearLayout.WrapContents = false;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.TabBaseStats);
            this.tabControl1.Controls.Add(this.TabDisappearParameters);
            this.tabControl1.Location = new System.Drawing.Point(14, 41);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(457, 697);
            this.tabControl1.TabIndex = 1;
            // 
            // TabBaseStats
            // 
            this.TabBaseStats.Controls.Add(this.BaseStatsLayout);
            this.TabBaseStats.Location = new System.Drawing.Point(4, 24);
            this.TabBaseStats.Name = "TabBaseStats";
            this.TabBaseStats.Padding = new System.Windows.Forms.Padding(3);
            this.TabBaseStats.Size = new System.Drawing.Size(449, 669);
            this.TabBaseStats.TabIndex = 0;
            this.TabBaseStats.Text = "Base Stats";
            this.TabBaseStats.UseVisualStyleBackColor = true;
            // 
            // TabDisappearParameters
            // 
            this.TabDisappearParameters.Controls.Add(this.DisappearLayout);
            this.TabDisappearParameters.Location = new System.Drawing.Point(4, 24);
            this.TabDisappearParameters.Name = "TabDisappearParameters";
            this.TabDisappearParameters.Padding = new System.Windows.Forms.Padding(3);
            this.TabDisappearParameters.Size = new System.Drawing.Size(407, 388);
            this.TabDisappearParameters.TabIndex = 1;
            this.TabDisappearParameters.Text = "Disappear Parameters";
            this.TabDisappearParameters.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(483, 750);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.SaveBEP);
            this.Controls.Add(this.LoadBEP);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "Form1";
            this.Text = "BEP Editor";
            this.tabControl1.ResumeLayout(false);
            this.TabBaseStats.ResumeLayout(false);
            this.TabDisappearParameters.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.FlowLayoutPanel BaseStatsLayout;
        private System.Windows.Forms.Button LoadBEP;
        private System.Windows.Forms.Button SaveBEP;
        private System.Windows.Forms.FlowLayoutPanel DisappearLayout;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage TabBaseStats;
        private System.Windows.Forms.TabPage TabDisappearParameters;
    }
}

