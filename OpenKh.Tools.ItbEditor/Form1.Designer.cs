
namespace OpenKh.Tools.ItbEditor
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
            this.LoadITBButton = new System.Windows.Forms.Button();
            this.SaveITBButton = new System.Windows.Forms.Button();
            this.ItbTabControl = new System.Windows.Forms.TabControl();
            this.TabDP = new System.Windows.Forms.TabPage();
            this.FlowDP = new System.Windows.Forms.FlowLayoutPanel();
            this.TabSW = new System.Windows.Forms.TabPage();
            this.FlowSW = new System.Windows.Forms.FlowLayoutPanel();
            this.TabCD = new System.Windows.Forms.TabPage();
            this.FlowCD = new System.Windows.Forms.FlowLayoutPanel();
            this.TabSB = new System.Windows.Forms.TabPage();
            this.FlowSB = new System.Windows.Forms.FlowLayoutPanel();
            this.TabYT = new System.Windows.Forms.TabPage();
            this.FlowYT = new System.Windows.Forms.FlowLayoutPanel();
            this.TabRG = new System.Windows.Forms.TabPage();
            this.FlowRG = new System.Windows.Forms.FlowLayoutPanel();
            this.TabJB = new System.Windows.Forms.TabPage();
            this.FlowJB = new System.Windows.Forms.FlowLayoutPanel();
            this.TabHE = new System.Windows.Forms.TabPage();
            this.FlowHE = new System.Windows.Forms.FlowLayoutPanel();
            this.TabLS = new System.Windows.Forms.TabPage();
            this.FlowLS = new System.Windows.Forms.FlowLayoutPanel();
            this.TabDI = new System.Windows.Forms.TabPage();
            this.FlowDI = new System.Windows.Forms.FlowLayoutPanel();
            this.TabPP = new System.Windows.Forms.TabPage();
            this.FlowPP = new System.Windows.Forms.FlowLayoutPanel();
            this.TabDC = new System.Windows.Forms.TabPage();
            this.FlowDC = new System.Windows.Forms.FlowLayoutPanel();
            this.TabKG = new System.Windows.Forms.TabPage();
            this.FlowKG = new System.Windows.Forms.FlowLayoutPanel();
            this.TabVS = new System.Windows.Forms.TabPage();
            this.FlowVS = new System.Windows.Forms.FlowLayoutPanel();
            this.TabBD = new System.Windows.Forms.TabPage();
            this.FlowBD = new System.Windows.Forms.FlowLayoutPanel();
            this.TabWM = new System.Windows.Forms.TabPage();
            this.FlowWM = new System.Windows.Forms.FlowLayoutPanel();
            this.NewChestButton = new System.Windows.Forms.Button();
            this.ItbTabControl.SuspendLayout();
            this.TabDP.SuspendLayout();
            this.TabSW.SuspendLayout();
            this.TabCD.SuspendLayout();
            this.TabSB.SuspendLayout();
            this.TabYT.SuspendLayout();
            this.TabRG.SuspendLayout();
            this.TabJB.SuspendLayout();
            this.TabHE.SuspendLayout();
            this.TabLS.SuspendLayout();
            this.TabDI.SuspendLayout();
            this.TabPP.SuspendLayout();
            this.TabDC.SuspendLayout();
            this.TabKG.SuspendLayout();
            this.TabVS.SuspendLayout();
            this.TabBD.SuspendLayout();
            this.TabWM.SuspendLayout();
            this.SuspendLayout();
            // 
            // LoadITBButton
            // 
            this.LoadITBButton.Font = new System.Drawing.Font("Segoe UI Historic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.LoadITBButton.Location = new System.Drawing.Point(13, 13);
            this.LoadITBButton.Name = "LoadITBButton";
            this.LoadITBButton.Size = new System.Drawing.Size(75, 23);
            this.LoadITBButton.TabIndex = 0;
            this.LoadITBButton.Text = "Load ITB";
            this.LoadITBButton.UseVisualStyleBackColor = true;
            this.LoadITBButton.Click += new System.EventHandler(this.LoadITCButton_Click);
            // 
            // SaveITBButton
            // 
            this.SaveITBButton.Enabled = false;
            this.SaveITBButton.Font = new System.Drawing.Font("Segoe UI Historic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.SaveITBButton.Location = new System.Drawing.Point(94, 13);
            this.SaveITBButton.Name = "SaveITBButton";
            this.SaveITBButton.Size = new System.Drawing.Size(75, 23);
            this.SaveITBButton.TabIndex = 1;
            this.SaveITBButton.Text = "Save as...";
            this.SaveITBButton.UseVisualStyleBackColor = true;
            this.SaveITBButton.Click += new System.EventHandler(this.SaveITCButton_Click);
            // 
            // ItbTabControl
            // 
            this.ItbTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ItbTabControl.Controls.Add(this.TabDP);
            this.ItbTabControl.Controls.Add(this.TabSW);
            this.ItbTabControl.Controls.Add(this.TabCD);
            this.ItbTabControl.Controls.Add(this.TabSB);
            this.ItbTabControl.Controls.Add(this.TabYT);
            this.ItbTabControl.Controls.Add(this.TabRG);
            this.ItbTabControl.Controls.Add(this.TabJB);
            this.ItbTabControl.Controls.Add(this.TabHE);
            this.ItbTabControl.Controls.Add(this.TabLS);
            this.ItbTabControl.Controls.Add(this.TabDI);
            this.ItbTabControl.Controls.Add(this.TabPP);
            this.ItbTabControl.Controls.Add(this.TabDC);
            this.ItbTabControl.Controls.Add(this.TabKG);
            this.ItbTabControl.Controls.Add(this.TabVS);
            this.ItbTabControl.Controls.Add(this.TabBD);
            this.ItbTabControl.Controls.Add(this.TabWM);
            this.ItbTabControl.ItemSize = new System.Drawing.Size(40, 20);
            this.ItbTabControl.Location = new System.Drawing.Point(12, 42);
            this.ItbTabControl.Name = "ItbTabControl";
            this.ItbTabControl.SelectedIndex = 0;
            this.ItbTabControl.Size = new System.Drawing.Size(645, 390);
            this.ItbTabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.ItbTabControl.TabIndex = 2;
            // 
            // TabDP
            // 
            this.TabDP.AutoScroll = true;
            this.TabDP.BackColor = System.Drawing.Color.White;
            this.TabDP.Controls.Add(this.FlowDP);
            this.TabDP.ForeColor = System.Drawing.SystemColors.ControlText;
            this.TabDP.Location = new System.Drawing.Point(4, 24);
            this.TabDP.Name = "TabDP";
            this.TabDP.Size = new System.Drawing.Size(637, 362);
            this.TabDP.TabIndex = 0;
            this.TabDP.Text = "DP";
            // 
            // FlowDP
            // 
            this.FlowDP.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowDP.AutoScroll = true;
            this.FlowDP.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowDP.Location = new System.Drawing.Point(3, 3);
            this.FlowDP.Name = "FlowDP";
            this.FlowDP.Size = new System.Drawing.Size(631, 356);
            this.FlowDP.TabIndex = 0;
            // 
            // TabSW
            // 
            this.TabSW.AutoScroll = true;
            this.TabSW.Controls.Add(this.FlowSW);
            this.TabSW.Location = new System.Drawing.Point(4, 24);
            this.TabSW.Name = "TabSW";
            this.TabSW.Size = new System.Drawing.Size(637, 362);
            this.TabSW.TabIndex = 1;
            this.TabSW.Text = "SW";
            this.TabSW.UseVisualStyleBackColor = true;
            // 
            // FlowSW
            // 
            this.FlowSW.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowSW.AutoScroll = true;
            this.FlowSW.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowSW.Location = new System.Drawing.Point(3, 3);
            this.FlowSW.Name = "FlowSW";
            this.FlowSW.Size = new System.Drawing.Size(631, 356);
            this.FlowSW.TabIndex = 1;
            // 
            // TabCD
            // 
            this.TabCD.AutoScroll = true;
            this.TabCD.Controls.Add(this.FlowCD);
            this.TabCD.Location = new System.Drawing.Point(4, 24);
            this.TabCD.Name = "TabCD";
            this.TabCD.Size = new System.Drawing.Size(637, 362);
            this.TabCD.TabIndex = 2;
            this.TabCD.Text = "CD";
            this.TabCD.UseVisualStyleBackColor = true;
            // 
            // FlowCD
            // 
            this.FlowCD.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowCD.AutoScroll = true;
            this.FlowCD.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowCD.Location = new System.Drawing.Point(3, 3);
            this.FlowCD.Name = "FlowCD";
            this.FlowCD.Size = new System.Drawing.Size(631, 356);
            this.FlowCD.TabIndex = 1;
            // 
            // TabSB
            // 
            this.TabSB.AutoScroll = true;
            this.TabSB.Controls.Add(this.FlowSB);
            this.TabSB.Location = new System.Drawing.Point(4, 24);
            this.TabSB.Name = "TabSB";
            this.TabSB.Size = new System.Drawing.Size(637, 362);
            this.TabSB.TabIndex = 3;
            this.TabSB.Text = "SB";
            this.TabSB.UseVisualStyleBackColor = true;
            // 
            // FlowSB
            // 
            this.FlowSB.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowSB.AutoScroll = true;
            this.FlowSB.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowSB.Location = new System.Drawing.Point(3, 3);
            this.FlowSB.Name = "FlowSB";
            this.FlowSB.Size = new System.Drawing.Size(631, 356);
            this.FlowSB.TabIndex = 1;
            // 
            // TabYT
            // 
            this.TabYT.AutoScroll = true;
            this.TabYT.Controls.Add(this.FlowYT);
            this.TabYT.Location = new System.Drawing.Point(4, 24);
            this.TabYT.Name = "TabYT";
            this.TabYT.Size = new System.Drawing.Size(637, 362);
            this.TabYT.TabIndex = 4;
            this.TabYT.Text = "YT";
            this.TabYT.UseVisualStyleBackColor = true;
            // 
            // FlowYT
            // 
            this.FlowYT.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowYT.AutoScroll = true;
            this.FlowYT.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowYT.Location = new System.Drawing.Point(3, 3);
            this.FlowYT.Name = "FlowYT";
            this.FlowYT.Size = new System.Drawing.Size(631, 356);
            this.FlowYT.TabIndex = 1;
            // 
            // TabRG
            // 
            this.TabRG.AutoScroll = true;
            this.TabRG.Controls.Add(this.FlowRG);
            this.TabRG.Location = new System.Drawing.Point(4, 24);
            this.TabRG.Name = "TabRG";
            this.TabRG.Size = new System.Drawing.Size(637, 362);
            this.TabRG.TabIndex = 5;
            this.TabRG.Text = "RG";
            this.TabRG.UseVisualStyleBackColor = true;
            // 
            // FlowRG
            // 
            this.FlowRG.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowRG.AutoScroll = true;
            this.FlowRG.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowRG.Location = new System.Drawing.Point(3, 3);
            this.FlowRG.Name = "FlowRG";
            this.FlowRG.Size = new System.Drawing.Size(631, 356);
            this.FlowRG.TabIndex = 1;
            // 
            // TabJB
            // 
            this.TabJB.AutoScroll = true;
            this.TabJB.Controls.Add(this.FlowJB);
            this.TabJB.Location = new System.Drawing.Point(4, 24);
            this.TabJB.Name = "TabJB";
            this.TabJB.Size = new System.Drawing.Size(637, 362);
            this.TabJB.TabIndex = 6;
            this.TabJB.Text = "JB";
            this.TabJB.UseVisualStyleBackColor = true;
            // 
            // FlowJB
            // 
            this.FlowJB.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowJB.AutoScroll = true;
            this.FlowJB.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowJB.Location = new System.Drawing.Point(3, 3);
            this.FlowJB.Name = "FlowJB";
            this.FlowJB.Size = new System.Drawing.Size(631, 356);
            this.FlowJB.TabIndex = 1;
            // 
            // TabHE
            // 
            this.TabHE.AutoScroll = true;
            this.TabHE.Controls.Add(this.FlowHE);
            this.TabHE.Location = new System.Drawing.Point(4, 24);
            this.TabHE.Name = "TabHE";
            this.TabHE.Size = new System.Drawing.Size(637, 362);
            this.TabHE.TabIndex = 7;
            this.TabHE.Text = "HE";
            this.TabHE.UseVisualStyleBackColor = true;
            // 
            // FlowHE
            // 
            this.FlowHE.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowHE.AutoScroll = true;
            this.FlowHE.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowHE.Location = new System.Drawing.Point(3, 3);
            this.FlowHE.Name = "FlowHE";
            this.FlowHE.Size = new System.Drawing.Size(631, 356);
            this.FlowHE.TabIndex = 1;
            // 
            // TabLS
            // 
            this.TabLS.AutoScroll = true;
            this.TabLS.Controls.Add(this.FlowLS);
            this.TabLS.Location = new System.Drawing.Point(4, 24);
            this.TabLS.Name = "TabLS";
            this.TabLS.Size = new System.Drawing.Size(637, 362);
            this.TabLS.TabIndex = 8;
            this.TabLS.Text = "LS";
            this.TabLS.UseVisualStyleBackColor = true;
            // 
            // FlowLS
            // 
            this.FlowLS.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowLS.AutoScroll = true;
            this.FlowLS.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowLS.Location = new System.Drawing.Point(3, 3);
            this.FlowLS.Name = "FlowLS";
            this.FlowLS.Size = new System.Drawing.Size(631, 356);
            this.FlowLS.TabIndex = 1;
            // 
            // TabDI
            // 
            this.TabDI.AutoScroll = true;
            this.TabDI.Controls.Add(this.FlowDI);
            this.TabDI.Location = new System.Drawing.Point(4, 24);
            this.TabDI.Name = "TabDI";
            this.TabDI.Size = new System.Drawing.Size(637, 362);
            this.TabDI.TabIndex = 9;
            this.TabDI.Text = "DI";
            this.TabDI.UseVisualStyleBackColor = true;
            // 
            // FlowDI
            // 
            this.FlowDI.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowDI.AutoScroll = true;
            this.FlowDI.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowDI.Location = new System.Drawing.Point(3, 3);
            this.FlowDI.Name = "FlowDI";
            this.FlowDI.Size = new System.Drawing.Size(631, 356);
            this.FlowDI.TabIndex = 1;
            // 
            // TabPP
            // 
            this.TabPP.AutoScroll = true;
            this.TabPP.Controls.Add(this.FlowPP);
            this.TabPP.Location = new System.Drawing.Point(4, 24);
            this.TabPP.Name = "TabPP";
            this.TabPP.Size = new System.Drawing.Size(637, 362);
            this.TabPP.TabIndex = 10;
            this.TabPP.Text = "PP";
            this.TabPP.UseVisualStyleBackColor = true;
            // 
            // FlowPP
            // 
            this.FlowPP.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowPP.AutoScroll = true;
            this.FlowPP.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowPP.Location = new System.Drawing.Point(3, 3);
            this.FlowPP.Name = "FlowPP";
            this.FlowPP.Size = new System.Drawing.Size(631, 356);
            this.FlowPP.TabIndex = 1;
            // 
            // TabDC
            // 
            this.TabDC.AutoScroll = true;
            this.TabDC.Controls.Add(this.FlowDC);
            this.TabDC.Location = new System.Drawing.Point(4, 24);
            this.TabDC.Name = "TabDC";
            this.TabDC.Size = new System.Drawing.Size(637, 362);
            this.TabDC.TabIndex = 11;
            this.TabDC.Text = "DC";
            this.TabDC.UseVisualStyleBackColor = true;
            // 
            // FlowDC
            // 
            this.FlowDC.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowDC.AutoScroll = true;
            this.FlowDC.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowDC.Location = new System.Drawing.Point(3, 3);
            this.FlowDC.Name = "FlowDC";
            this.FlowDC.Size = new System.Drawing.Size(631, 356);
            this.FlowDC.TabIndex = 1;
            // 
            // TabKG
            // 
            this.TabKG.AutoScroll = true;
            this.TabKG.Controls.Add(this.FlowKG);
            this.TabKG.Location = new System.Drawing.Point(4, 24);
            this.TabKG.Name = "TabKG";
            this.TabKG.Size = new System.Drawing.Size(637, 362);
            this.TabKG.TabIndex = 12;
            this.TabKG.Text = "KG";
            this.TabKG.UseVisualStyleBackColor = true;
            // 
            // FlowKG
            // 
            this.FlowKG.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowKG.AutoScroll = true;
            this.FlowKG.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowKG.Location = new System.Drawing.Point(3, 3);
            this.FlowKG.Name = "FlowKG";
            this.FlowKG.Size = new System.Drawing.Size(631, 356);
            this.FlowKG.TabIndex = 1;
            // 
            // TabVS
            // 
            this.TabVS.AutoScroll = true;
            this.TabVS.Controls.Add(this.FlowVS);
            this.TabVS.Location = new System.Drawing.Point(4, 24);
            this.TabVS.Name = "TabVS";
            this.TabVS.Size = new System.Drawing.Size(637, 362);
            this.TabVS.TabIndex = 13;
            this.TabVS.Text = "VS";
            this.TabVS.UseVisualStyleBackColor = true;
            // 
            // FlowVS
            // 
            this.FlowVS.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowVS.AutoScroll = true;
            this.FlowVS.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowVS.Location = new System.Drawing.Point(3, 3);
            this.FlowVS.Name = "FlowVS";
            this.FlowVS.Size = new System.Drawing.Size(631, 356);
            this.FlowVS.TabIndex = 1;
            // 
            // TabBD
            // 
            this.TabBD.AutoScroll = true;
            this.TabBD.Controls.Add(this.FlowBD);
            this.TabBD.Location = new System.Drawing.Point(4, 24);
            this.TabBD.Name = "TabBD";
            this.TabBD.Size = new System.Drawing.Size(637, 362);
            this.TabBD.TabIndex = 14;
            this.TabBD.Text = "BD";
            this.TabBD.UseVisualStyleBackColor = true;
            // 
            // FlowBD
            // 
            this.FlowBD.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowBD.AutoScroll = true;
            this.FlowBD.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowBD.Location = new System.Drawing.Point(3, 3);
            this.FlowBD.Name = "FlowBD";
            this.FlowBD.Size = new System.Drawing.Size(631, 356);
            this.FlowBD.TabIndex = 1;
            // 
            // TabWM
            // 
            this.TabWM.AutoScroll = true;
            this.TabWM.Controls.Add(this.FlowWM);
            this.TabWM.Location = new System.Drawing.Point(4, 24);
            this.TabWM.Name = "TabWM";
            this.TabWM.Size = new System.Drawing.Size(637, 362);
            this.TabWM.TabIndex = 15;
            this.TabWM.Text = "WM";
            this.TabWM.UseVisualStyleBackColor = true;
            // 
            // FlowWM
            // 
            this.FlowWM.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowWM.AutoScroll = true;
            this.FlowWM.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowWM.Location = new System.Drawing.Point(3, 3);
            this.FlowWM.Name = "FlowWM";
            this.FlowWM.Size = new System.Drawing.Size(631, 356);
            this.FlowWM.TabIndex = 1;
            // 
            // NewChestButton
            // 
            this.NewChestButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NewChestButton.Enabled = false;
            this.NewChestButton.Location = new System.Drawing.Point(493, 13);
            this.NewChestButton.Name = "NewChestButton";
            this.NewChestButton.Size = new System.Drawing.Size(160, 23);
            this.NewChestButton.TabIndex = 3;
            this.NewChestButton.Text = "Add Chest to this world";
            this.NewChestButton.UseVisualStyleBackColor = true;
            this.NewChestButton.Click += new System.EventHandler(this.NewChestButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(664, 441);
            this.Controls.Add(this.NewChestButton);
            this.Controls.Add(this.ItbTabControl);
            this.Controls.Add(this.SaveITBButton);
            this.Controls.Add(this.LoadITBButton);
            this.Font = new System.Drawing.Font("Segoe UI Historic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Name = "Form1";
            this.Text = "ITB Editor (Item Treasure Box)";
            this.ItbTabControl.ResumeLayout(false);
            this.TabDP.ResumeLayout(false);
            this.TabSW.ResumeLayout(false);
            this.TabCD.ResumeLayout(false);
            this.TabSB.ResumeLayout(false);
            this.TabYT.ResumeLayout(false);
            this.TabRG.ResumeLayout(false);
            this.TabJB.ResumeLayout(false);
            this.TabHE.ResumeLayout(false);
            this.TabLS.ResumeLayout(false);
            this.TabDI.ResumeLayout(false);
            this.TabPP.ResumeLayout(false);
            this.TabDC.ResumeLayout(false);
            this.TabKG.ResumeLayout(false);
            this.TabVS.ResumeLayout(false);
            this.TabBD.ResumeLayout(false);
            this.TabWM.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button LoadITBButton;
        private System.Windows.Forms.Button SaveITBButton;
        private System.Windows.Forms.TabControl ItbTabControl;
        private System.Windows.Forms.TabPage TabDP;
        private System.Windows.Forms.TabPage TabSW;
        private System.Windows.Forms.TabPage TabCD;
        private System.Windows.Forms.TabPage TabSB;
        private System.Windows.Forms.TabPage TabYT;
        private System.Windows.Forms.TabPage TabRG;
        private System.Windows.Forms.TabPage TabJB;
        private System.Windows.Forms.TabPage TabHE;
        private System.Windows.Forms.TabPage TabLS;
        private System.Windows.Forms.TabPage TabDI;
        private System.Windows.Forms.TabPage TabPP;
        private System.Windows.Forms.TabPage TabDC;
        private System.Windows.Forms.TabPage TabKG;
        private System.Windows.Forms.TabPage TabVS;
        private System.Windows.Forms.TabPage TabBD;
        private System.Windows.Forms.TabPage TabWM;
        private System.Windows.Forms.FlowLayoutPanel FlowDP;
        private System.Windows.Forms.FlowLayoutPanel FlowSW;
        private System.Windows.Forms.FlowLayoutPanel FlowCD;
        private System.Windows.Forms.FlowLayoutPanel FlowSB;
        private System.Windows.Forms.FlowLayoutPanel FlowYT;
        private System.Windows.Forms.FlowLayoutPanel FlowRG;
        private System.Windows.Forms.FlowLayoutPanel FlowJB;
        private System.Windows.Forms.FlowLayoutPanel FlowHE;
        private System.Windows.Forms.FlowLayoutPanel FlowLS;
        private System.Windows.Forms.FlowLayoutPanel FlowDI;
        private System.Windows.Forms.FlowLayoutPanel FlowPP;
        private System.Windows.Forms.FlowLayoutPanel FlowDC;
        private System.Windows.Forms.FlowLayoutPanel FlowKG;
        private System.Windows.Forms.FlowLayoutPanel FlowVS;
        private System.Windows.Forms.FlowLayoutPanel FlowBD;
        private System.Windows.Forms.FlowLayoutPanel FlowWM;
        private System.Windows.Forms.Button NewChestButton;
    }
}

