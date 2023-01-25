
namespace OpenKh.Tools.MissionEditor
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
            this.LoadMissionButton = new System.Windows.Forms.Button();
            this.SaveMissionButton = new System.Windows.Forms.Button();
            this.FlowMission = new System.Windows.Forms.FlowLayoutPanel();
            this.AddMissionButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LoadMissionButton
            // 
            this.LoadMissionButton.Location = new System.Drawing.Point(13, 13);
            this.LoadMissionButton.Name = "LoadMissionButton";
            this.LoadMissionButton.Size = new System.Drawing.Size(88, 27);
            this.LoadMissionButton.TabIndex = 0;
            this.LoadMissionButton.Text = "Load Mission";
            this.LoadMissionButton.UseVisualStyleBackColor = true;
            this.LoadMissionButton.Click += new System.EventHandler(this.LoadMissionButton_Click);
            // 
            // SaveMissionButton
            // 
            this.SaveMissionButton.Enabled = false;
            this.SaveMissionButton.Location = new System.Drawing.Point(107, 12);
            this.SaveMissionButton.Name = "SaveMissionButton";
            this.SaveMissionButton.Size = new System.Drawing.Size(83, 28);
            this.SaveMissionButton.TabIndex = 1;
            this.SaveMissionButton.Text = "Save as...";
            this.SaveMissionButton.UseVisualStyleBackColor = true;
            this.SaveMissionButton.Click += new System.EventHandler(this.SaveMissionButton_Click);
            // 
            // FlowMission
            // 
            this.FlowMission.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowMission.AutoScroll = true;
            this.FlowMission.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.FlowMission.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowMission.Location = new System.Drawing.Point(13, 47);
            this.FlowMission.Name = "FlowMission";
            this.FlowMission.Size = new System.Drawing.Size(681, 391);
            this.FlowMission.TabIndex = 2;
            // 
            // AddMissionButton
            // 
            this.AddMissionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddMissionButton.Enabled = false;
            this.AddMissionButton.Location = new System.Drawing.Point(573, 12);
            this.AddMissionButton.Name = "AddMissionButton";
            this.AddMissionButton.Size = new System.Drawing.Size(121, 28);
            this.AddMissionButton.TabIndex = 3;
            this.AddMissionButton.Text = "Add Mission";
            this.AddMissionButton.UseVisualStyleBackColor = true;
            this.AddMissionButton.Click += new System.EventHandler(this.AddMissionButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(706, 450);
            this.Controls.Add(this.AddMissionButton);
            this.Controls.Add(this.FlowMission);
            this.Controls.Add(this.SaveMissionButton);
            this.Controls.Add(this.LoadMissionButton);
            this.Name = "Form1";
            this.Text = "Mission Editor";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button LoadMissionButton;
        private System.Windows.Forms.Button SaveMissionButton;
        private System.Windows.Forms.FlowLayoutPanel FlowMission;
        private System.Windows.Forms.Button AddMissionButton;
    }
}

