
namespace OpenKh.Tools.IteEditor
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
            this.LoadITEButton = new System.Windows.Forms.Button();
            this.ITETabControl = new System.Windows.Forms.TabControl();
            this.TabWeapons = new System.Windows.Forms.TabPage();
            this.FlowWeapons = new System.Windows.Forms.FlowLayoutPanel();
            this.TabFlavors = new System.Windows.Forms.TabPage();
            this.FlowFlavors = new System.Windows.Forms.FlowLayoutPanel();
            this.TabKeyItem = new System.Windows.Forms.TabPage();
            this.FlowKeyItem = new System.Windows.Forms.FlowLayoutPanel();
            this.TabKeyItemHide = new System.Windows.Forms.TabPage();
            this.FlowKeyItemHide = new System.Windows.Forms.FlowLayoutPanel();
            this.TabSynthesis = new System.Windows.Forms.TabPage();
            this.FlowSynthesis = new System.Windows.Forms.FlowLayoutPanel();
            this.ITETabControl.SuspendLayout();
            this.TabWeapons.SuspendLayout();
            this.TabFlavors.SuspendLayout();
            this.TabKeyItem.SuspendLayout();
            this.TabKeyItemHide.SuspendLayout();
            this.TabSynthesis.SuspendLayout();
            this.SuspendLayout();
            // 
            // LoadITEButton
            // 
            this.LoadITEButton.Font = new System.Drawing.Font("Segoe UI Historic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.LoadITEButton.Location = new System.Drawing.Point(19, 22);
            this.LoadITEButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.LoadITEButton.Name = "LoadITEButton";
            this.LoadITEButton.Size = new System.Drawing.Size(107, 38);
            this.LoadITEButton.TabIndex = 0;
            this.LoadITEButton.Text = "Load ITE";
            this.LoadITEButton.UseVisualStyleBackColor = true;
            this.LoadITEButton.Click += new System.EventHandler(this.LoadITEButton_Click);
            // 
            // ITETabControl
            // 
            this.ITETabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ITETabControl.Controls.Add(this.TabWeapons);
            this.ITETabControl.Controls.Add(this.TabFlavors);
            this.ITETabControl.Controls.Add(this.TabKeyItem);
            this.ITETabControl.Controls.Add(this.TabKeyItemHide);
            this.ITETabControl.Controls.Add(this.TabSynthesis);
            this.ITETabControl.ItemSize = new System.Drawing.Size(94, 30);
            this.ITETabControl.Location = new System.Drawing.Point(0, 83);
            this.ITETabControl.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ITETabControl.Name = "ITETabControl";
            this.ITETabControl.Padding = new System.Drawing.Point(10, 3);
            this.ITETabControl.SelectedIndex = 0;
            this.ITETabControl.Size = new System.Drawing.Size(824, 625);
            this.ITETabControl.TabIndex = 2;
            // 
            // TabWeapons
            // 
            this.TabWeapons.AutoScroll = true;
            this.TabWeapons.Controls.Add(this.FlowWeapons);
            this.TabWeapons.Font = new System.Drawing.Font("Segoe UI Historic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.TabWeapons.Location = new System.Drawing.Point(4, 34);
            this.TabWeapons.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TabWeapons.Name = "TabWeapons";
            this.TabWeapons.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TabWeapons.Size = new System.Drawing.Size(816, 587);
            this.TabWeapons.TabIndex = 0;
            this.TabWeapons.Text = "Weapons";
            this.TabWeapons.UseVisualStyleBackColor = true;
            // 
            // FlowWeapons
            // 
            this.FlowWeapons.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowWeapons.AutoScroll = true;
            this.FlowWeapons.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.FlowWeapons.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowWeapons.Location = new System.Drawing.Point(4, 5);
            this.FlowWeapons.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FlowWeapons.Name = "FlowWeapons";
            this.FlowWeapons.Size = new System.Drawing.Size(803, 566);
            this.FlowWeapons.TabIndex = 0;
            // 
            // TabFlavors
            // 
            this.TabFlavors.AutoScroll = true;
            this.TabFlavors.Controls.Add(this.FlowFlavors);
            this.TabFlavors.Font = new System.Drawing.Font("Segoe UI Historic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.TabFlavors.Location = new System.Drawing.Point(4, 34);
            this.TabFlavors.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TabFlavors.Name = "TabFlavors";
            this.TabFlavors.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TabFlavors.Size = new System.Drawing.Size(816, 587);
            this.TabFlavors.TabIndex = 1;
            this.TabFlavors.Text = "Flavors";
            this.TabFlavors.UseVisualStyleBackColor = true;
            // 
            // FlowFlavors
            // 
            this.FlowFlavors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowFlavors.AutoScroll = true;
            this.FlowFlavors.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.FlowFlavors.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowFlavors.Location = new System.Drawing.Point(4, 5);
            this.FlowFlavors.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FlowFlavors.Name = "FlowFlavors";
            this.FlowFlavors.Size = new System.Drawing.Size(797, 560);
            this.FlowFlavors.TabIndex = 1;
            // 
            // TabKeyItem
            // 
            this.TabKeyItem.AutoScroll = true;
            this.TabKeyItem.Controls.Add(this.FlowKeyItem);
            this.TabKeyItem.Font = new System.Drawing.Font("Segoe UI Historic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.TabKeyItem.Location = new System.Drawing.Point(4, 34);
            this.TabKeyItem.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TabKeyItem.Name = "TabKeyItem";
            this.TabKeyItem.Size = new System.Drawing.Size(816, 587);
            this.TabKeyItem.TabIndex = 2;
            this.TabKeyItem.Text = "Key Item";
            this.TabKeyItem.UseVisualStyleBackColor = true;
            // 
            // FlowKeyItem
            // 
            this.FlowKeyItem.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowKeyItem.AutoScroll = true;
            this.FlowKeyItem.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.FlowKeyItem.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowKeyItem.Location = new System.Drawing.Point(4, 5);
            this.FlowKeyItem.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FlowKeyItem.Name = "FlowKeyItem";
            this.FlowKeyItem.Size = new System.Drawing.Size(797, 560);
            this.FlowKeyItem.TabIndex = 1;
            // 
            // TabKeyItemHide
            // 
            this.TabKeyItemHide.AutoScroll = true;
            this.TabKeyItemHide.Controls.Add(this.FlowKeyItemHide);
            this.TabKeyItemHide.Font = new System.Drawing.Font("Segoe UI Historic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.TabKeyItemHide.Location = new System.Drawing.Point(4, 34);
            this.TabKeyItemHide.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TabKeyItemHide.Name = "TabKeyItemHide";
            this.TabKeyItemHide.Size = new System.Drawing.Size(816, 587);
            this.TabKeyItemHide.TabIndex = 3;
            this.TabKeyItemHide.Text = "Key Item Hide";
            this.TabKeyItemHide.UseVisualStyleBackColor = true;
            // 
            // FlowKeyItemHide
            // 
            this.FlowKeyItemHide.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowKeyItemHide.AutoScroll = true;
            this.FlowKeyItemHide.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.FlowKeyItemHide.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowKeyItemHide.Location = new System.Drawing.Point(11, 5);
            this.FlowKeyItemHide.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FlowKeyItemHide.Name = "FlowKeyItemHide";
            this.FlowKeyItemHide.Size = new System.Drawing.Size(790, 560);
            this.FlowKeyItemHide.TabIndex = 1;
            // 
            // TabSynthesis
            // 
            this.TabSynthesis.AutoScroll = true;
            this.TabSynthesis.Controls.Add(this.FlowSynthesis);
            this.TabSynthesis.Font = new System.Drawing.Font("Segoe UI Historic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.TabSynthesis.Location = new System.Drawing.Point(4, 34);
            this.TabSynthesis.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TabSynthesis.Name = "TabSynthesis";
            this.TabSynthesis.Size = new System.Drawing.Size(816, 587);
            this.TabSynthesis.TabIndex = 4;
            this.TabSynthesis.Text = "Synthesis";
            this.TabSynthesis.UseVisualStyleBackColor = true;
            // 
            // FlowSynthesis
            // 
            this.FlowSynthesis.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowSynthesis.AutoScroll = true;
            this.FlowSynthesis.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.FlowSynthesis.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.FlowSynthesis.Location = new System.Drawing.Point(4, 5);
            this.FlowSynthesis.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.FlowSynthesis.Name = "FlowSynthesis";
            this.FlowSynthesis.Size = new System.Drawing.Size(797, 560);
            this.FlowSynthesis.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 708);
            this.Controls.Add(this.ITETabControl);
            this.Controls.Add(this.LoadITEButton);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "MainForm";
            this.Text = "ITE Viewer";
            this.ITETabControl.ResumeLayout(false);
            this.TabWeapons.ResumeLayout(false);
            this.TabFlavors.ResumeLayout(false);
            this.TabKeyItem.ResumeLayout(false);
            this.TabKeyItemHide.ResumeLayout(false);
            this.TabSynthesis.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button LoadITEButton;
        private System.Windows.Forms.TabControl ITETabControl;
        private System.Windows.Forms.TabPage TabWeapons;
        private System.Windows.Forms.FlowLayoutPanel FlowWeapons;
        private System.Windows.Forms.TabPage TabFlavors;
        private System.Windows.Forms.FlowLayoutPanel FlowFlavors;
        private System.Windows.Forms.TabPage TabKeyItem;
        private System.Windows.Forms.FlowLayoutPanel FlowKeyItem;
        private System.Windows.Forms.TabPage TabKeyItemHide;
        private System.Windows.Forms.FlowLayoutPanel FlowKeyItemHide;
        private System.Windows.Forms.TabPage TabSynthesis;
        private System.Windows.Forms.FlowLayoutPanel FlowSynthesis;
    }
}

