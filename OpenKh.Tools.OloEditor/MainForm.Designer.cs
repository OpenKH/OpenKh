
namespace OpenKh.Tools.OloEditor
{
    partial class MainForm
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
            this.LoadOLOButton = new System.Windows.Forms.Button();
            this.SaveOLOButton = new System.Windows.Forms.Button();
            this.TabControlOLO = new System.Windows.Forms.TabControl();
            this.TabObjects = new System.Windows.Forms.TabPage();
            this.FlowObjects = new System.Windows.Forms.FlowLayoutPanel();
            this.TabFilePath = new System.Windows.Forms.TabPage();
            this.FlowFiles = new System.Windows.Forms.FlowLayoutPanel();
            this.TabScripts = new System.Windows.Forms.TabPage();
            this.FlowScripts = new System.Windows.Forms.FlowLayoutPanel();
            this.TabMissions = new System.Windows.Forms.TabPage();
            this.FlowMissions = new System.Windows.Forms.FlowLayoutPanel();
            this.TabTriggers = new System.Windows.Forms.TabPage();
            this.FlowTriggers = new System.Windows.Forms.FlowLayoutPanel();
            this.TabLayout = new System.Windows.Forms.TabPage();
            this.FlowLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.OLOFlagsGBox = new System.Windows.Forms.GroupBox();
            this.EnemyFlag = new System.Windows.Forms.CheckBox();
            this.GimmickFlag = new System.Windows.Forms.CheckBox();
            this.NPCFlag = new System.Windows.Forms.CheckBox();
            this.PlayerFlag = new System.Windows.Forms.CheckBox();
            this.EventTriggerFlag = new System.Windows.Forms.CheckBox();
            this.TabControlOLO.SuspendLayout();
            this.TabObjects.SuspendLayout();
            this.TabFilePath.SuspendLayout();
            this.TabScripts.SuspendLayout();
            this.TabMissions.SuspendLayout();
            this.TabTriggers.SuspendLayout();
            this.TabLayout.SuspendLayout();
            this.OLOFlagsGBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // LoadOLOButton
            // 
            this.LoadOLOButton.Location = new System.Drawing.Point(12, 12);
            this.LoadOLOButton.Name = "LoadOLOButton";
            this.LoadOLOButton.Size = new System.Drawing.Size(75, 30);
            this.LoadOLOButton.TabIndex = 0;
            this.LoadOLOButton.Text = "Load OLO";
            this.LoadOLOButton.UseVisualStyleBackColor = true;
            this.LoadOLOButton.Click += new System.EventHandler(this.LoadOLOButton_Click);
            // 
            // SaveOLOButton
            // 
            this.SaveOLOButton.Enabled = false;
            this.SaveOLOButton.Location = new System.Drawing.Point(93, 12);
            this.SaveOLOButton.Name = "SaveOLOButton";
            this.SaveOLOButton.Size = new System.Drawing.Size(75, 30);
            this.SaveOLOButton.TabIndex = 1;
            this.SaveOLOButton.Text = "Save as...";
            this.SaveOLOButton.UseVisualStyleBackColor = true;
            this.SaveOLOButton.Click += new System.EventHandler(this.SaveOLOButton_Click);
            // 
            // TabControlOLO
            // 
            this.TabControlOLO.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TabControlOLO.Controls.Add(this.TabObjects);
            this.TabControlOLO.Controls.Add(this.TabFilePath);
            this.TabControlOLO.Controls.Add(this.TabScripts);
            this.TabControlOLO.Controls.Add(this.TabMissions);
            this.TabControlOLO.Controls.Add(this.TabTriggers);
            this.TabControlOLO.Controls.Add(this.TabLayout);
            this.TabControlOLO.ItemSize = new System.Drawing.Size(125, 20);
            this.TabControlOLO.Location = new System.Drawing.Point(12, 54);
            this.TabControlOLO.Name = "TabControlOLO";
            this.TabControlOLO.SelectedIndex = 0;
            this.TabControlOLO.Size = new System.Drawing.Size(960, 545);
            this.TabControlOLO.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.TabControlOLO.TabIndex = 2;
            // 
            // TabObjects
            // 
            this.TabObjects.BackColor = System.Drawing.Color.Transparent;
            this.TabObjects.Controls.Add(this.FlowObjects);
            this.TabObjects.ForeColor = System.Drawing.SystemColors.ControlText;
            this.TabObjects.Location = new System.Drawing.Point(4, 24);
            this.TabObjects.Name = "TabObjects";
            this.TabObjects.Padding = new System.Windows.Forms.Padding(3);
            this.TabObjects.Size = new System.Drawing.Size(952, 517);
            this.TabObjects.TabIndex = 0;
            this.TabObjects.Text = "Objects";
            // 
            // FlowObjects
            // 
            this.FlowObjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowObjects.AutoSize = true;
            this.FlowObjects.Location = new System.Drawing.Point(3, 3);
            this.FlowObjects.Name = "FlowObjects";
            this.FlowObjects.Size = new System.Drawing.Size(946, 514);
            this.FlowObjects.TabIndex = 0;
            // 
            // TabFilePath
            // 
            this.TabFilePath.Controls.Add(this.FlowFiles);
            this.TabFilePath.Location = new System.Drawing.Point(4, 24);
            this.TabFilePath.Name = "TabFilePath";
            this.TabFilePath.Padding = new System.Windows.Forms.Padding(3);
            this.TabFilePath.Size = new System.Drawing.Size(952, 530);
            this.TabFilePath.TabIndex = 1;
            this.TabFilePath.Text = "Files";
            // 
            // FlowFiles
            // 
            this.FlowFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowFiles.AutoScroll = true;
            this.FlowFiles.Location = new System.Drawing.Point(0, 0);
            this.FlowFiles.Name = "FlowFiles";
            this.FlowFiles.Size = new System.Drawing.Size(949, 527);
            this.FlowFiles.TabIndex = 0;
            // 
            // TabScripts
            // 
            this.TabScripts.Controls.Add(this.FlowScripts);
            this.TabScripts.Location = new System.Drawing.Point(4, 24);
            this.TabScripts.Name = "TabScripts";
            this.TabScripts.Size = new System.Drawing.Size(952, 530);
            this.TabScripts.TabIndex = 2;
            this.TabScripts.Text = "Scripts";
            // 
            // FlowScripts
            // 
            this.FlowScripts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowScripts.AutoScroll = true;
            this.FlowScripts.Location = new System.Drawing.Point(3, 3);
            this.FlowScripts.Name = "FlowScripts";
            this.FlowScripts.Size = new System.Drawing.Size(744, 478);
            this.FlowScripts.TabIndex = 0;
            // 
            // TabMissions
            // 
            this.TabMissions.Controls.Add(this.FlowMissions);
            this.TabMissions.Location = new System.Drawing.Point(4, 24);
            this.TabMissions.Name = "TabMissions";
            this.TabMissions.Size = new System.Drawing.Size(952, 530);
            this.TabMissions.TabIndex = 3;
            this.TabMissions.Text = "Missions";
            // 
            // FlowMissions
            // 
            this.FlowMissions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowMissions.AutoScroll = true;
            this.FlowMissions.Location = new System.Drawing.Point(3, 3);
            this.FlowMissions.Name = "FlowMissions";
            this.FlowMissions.Size = new System.Drawing.Size(744, 478);
            this.FlowMissions.TabIndex = 0;
            // 
            // TabTriggers
            // 
            this.TabTriggers.AutoScroll = true;
            this.TabTriggers.Controls.Add(this.FlowTriggers);
            this.TabTriggers.Location = new System.Drawing.Point(4, 24);
            this.TabTriggers.Name = "TabTriggers";
            this.TabTriggers.Size = new System.Drawing.Size(952, 530);
            this.TabTriggers.TabIndex = 4;
            this.TabTriggers.Text = "Triggers";
            // 
            // FlowTriggers
            // 
            this.FlowTriggers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowTriggers.AutoScroll = true;
            this.FlowTriggers.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowTriggers.Location = new System.Drawing.Point(0, 0);
            this.FlowTriggers.Name = "FlowTriggers";
            this.FlowTriggers.Size = new System.Drawing.Size(949, 527);
            this.FlowTriggers.TabIndex = 0;
            this.FlowTriggers.WrapContents = false;
            // 
            // TabLayout
            // 
            this.TabLayout.Controls.Add(this.FlowLayout);
            this.TabLayout.Location = new System.Drawing.Point(4, 24);
            this.TabLayout.Name = "TabLayout";
            this.TabLayout.Size = new System.Drawing.Size(952, 530);
            this.TabLayout.TabIndex = 5;
            this.TabLayout.Text = "Layout";
            // 
            // FlowLayout
            // 
            this.FlowLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowLayout.AutoScroll = true;
            this.FlowLayout.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowLayout.Location = new System.Drawing.Point(3, 3);
            this.FlowLayout.Name = "FlowLayout";
            this.FlowLayout.Size = new System.Drawing.Size(946, 524);
            this.FlowLayout.TabIndex = 0;
            this.FlowLayout.WrapContents = false;
            // 
            // OLOFlagsGBox
            // 
            this.OLOFlagsGBox.Controls.Add(this.EventTriggerFlag);
            this.OLOFlagsGBox.Controls.Add(this.PlayerFlag);
            this.OLOFlagsGBox.Controls.Add(this.NPCFlag);
            this.OLOFlagsGBox.Controls.Add(this.GimmickFlag);
            this.OLOFlagsGBox.Controls.Add(this.EnemyFlag);
            this.OLOFlagsGBox.Enabled = false;
            this.OLOFlagsGBox.Location = new System.Drawing.Point(174, 5);
            this.OLOFlagsGBox.Name = "OLOFlagsGBox";
            this.OLOFlagsGBox.Size = new System.Drawing.Size(402, 43);
            this.OLOFlagsGBox.TabIndex = 3;
            this.OLOFlagsGBox.TabStop = false;
            this.OLOFlagsGBox.Text = "Flags";
            // 
            // EnemyFlag
            // 
            this.EnemyFlag.AutoSize = true;
            this.EnemyFlag.Location = new System.Drawing.Point(26, 18);
            this.EnemyFlag.Name = "EnemyFlag";
            this.EnemyFlag.Size = new System.Drawing.Size(62, 19);
            this.EnemyFlag.TabIndex = 0;
            this.EnemyFlag.Text = "Enemy";
            this.EnemyFlag.UseVisualStyleBackColor = true;
            // 
            // GimmickFlag
            // 
            this.GimmickFlag.AutoSize = true;
            this.GimmickFlag.Location = new System.Drawing.Point(94, 18);
            this.GimmickFlag.Name = "GimmickFlag";
            this.GimmickFlag.Size = new System.Drawing.Size(74, 19);
            this.GimmickFlag.TabIndex = 0;
            this.GimmickFlag.Text = "Gimmick";
            this.GimmickFlag.UseVisualStyleBackColor = true;
            // 
            // NPCFlag
            // 
            this.NPCFlag.AutoSize = true;
            this.NPCFlag.Location = new System.Drawing.Point(174, 18);
            this.NPCFlag.Name = "NPCFlag";
            this.NPCFlag.Size = new System.Drawing.Size(50, 19);
            this.NPCFlag.TabIndex = 0;
            this.NPCFlag.Text = "NPC";
            this.NPCFlag.UseVisualStyleBackColor = true;
            // 
            // PlayerFlag
            // 
            this.PlayerFlag.AutoSize = true;
            this.PlayerFlag.Location = new System.Drawing.Point(230, 18);
            this.PlayerFlag.Name = "PlayerFlag";
            this.PlayerFlag.Size = new System.Drawing.Size(58, 19);
            this.PlayerFlag.TabIndex = 0;
            this.PlayerFlag.Text = "Player";
            this.PlayerFlag.UseVisualStyleBackColor = true;
            // 
            // EventTriggerFlag
            // 
            this.EventTriggerFlag.AutoSize = true;
            this.EventTriggerFlag.Location = new System.Drawing.Point(298, 18);
            this.EventTriggerFlag.Name = "EventTriggerFlag";
            this.EventTriggerFlag.Size = new System.Drawing.Size(94, 19);
            this.EventTriggerFlag.TabIndex = 0;
            this.EventTriggerFlag.Text = "Event Trigger";
            this.EventTriggerFlag.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 611);
            this.Controls.Add(this.OLOFlagsGBox);
            this.Controls.Add(this.TabControlOLO);
            this.Controls.Add(this.SaveOLOButton);
            this.Controls.Add(this.LoadOLOButton);
            this.Name = "MainForm";
            this.Text = "OLO Editor";
            this.TabControlOLO.ResumeLayout(false);
            this.TabObjects.ResumeLayout(false);
            this.TabObjects.PerformLayout();
            this.TabFilePath.ResumeLayout(false);
            this.TabScripts.ResumeLayout(false);
            this.TabMissions.ResumeLayout(false);
            this.TabTriggers.ResumeLayout(false);
            this.TabLayout.ResumeLayout(false);
            this.OLOFlagsGBox.ResumeLayout(false);
            this.OLOFlagsGBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button LoadOLOButton;
        private System.Windows.Forms.Button SaveOLOButton;
        private System.Windows.Forms.TabControl TabControlOLO;
        private System.Windows.Forms.TabPage TabObjects;
        private System.Windows.Forms.TabPage TabFilePath;
        private System.Windows.Forms.TabPage TabScripts;
        private System.Windows.Forms.TabPage TabMissions;
        private System.Windows.Forms.TabPage TabTriggers;
        private System.Windows.Forms.TabPage TabLayout;
        private System.Windows.Forms.FlowLayoutPanel FlowObjects;
        private System.Windows.Forms.FlowLayoutPanel FlowFiles;
        private System.Windows.Forms.FlowLayoutPanel FlowScripts;
        private System.Windows.Forms.FlowLayoutPanel FlowMissions;
        private System.Windows.Forms.FlowLayoutPanel FlowTriggers;
        private System.Windows.Forms.FlowLayoutPanel FlowLayout;
        private System.Windows.Forms.GroupBox OLOFlagsGBox;
        private System.Windows.Forms.CheckBox EventTriggerFlag;
        private System.Windows.Forms.CheckBox PlayerFlag;
        private System.Windows.Forms.CheckBox NPCFlag;
        private System.Windows.Forms.CheckBox GimmickFlag;
        private System.Windows.Forms.CheckBox EnemyFlag;
    }
}

