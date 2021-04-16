
namespace OpenKh.Tools.PMORetarget
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
            this.PMOSourceButton = new System.Windows.Forms.Button();
            this.ApplyRetargettingButton = new System.Windows.Forms.Button();
            this.PMOTargetButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // PMOSourceButton
            // 
            this.PMOSourceButton.Location = new System.Drawing.Point(13, 13);
            this.PMOSourceButton.Name = "PMOSourceButton";
            this.PMOSourceButton.Size = new System.Drawing.Size(145, 38);
            this.PMOSourceButton.TabIndex = 0;
            this.PMOSourceButton.Text = "Choose source BBS PMO";
            this.PMOSourceButton.UseVisualStyleBackColor = true;
            this.PMOSourceButton.Click += new System.EventHandler(this.PMOSourceButton_Click);
            // 
            // ApplyRetargettingButton
            // 
            this.ApplyRetargettingButton.Location = new System.Drawing.Point(223, 13);
            this.ApplyRetargettingButton.Name = "ApplyRetargettingButton";
            this.ApplyRetargettingButton.Size = new System.Drawing.Size(129, 38);
            this.ApplyRetargettingButton.TabIndex = 1;
            this.ApplyRetargettingButton.Text = "APPLY RETARGETTING";
            this.ApplyRetargettingButton.UseVisualStyleBackColor = true;
            this.ApplyRetargettingButton.Click += new System.EventHandler(this.ApplyRetargettingButton_Click);
            // 
            // PMOTargetButton
            // 
            this.PMOTargetButton.Location = new System.Drawing.Point(424, 12);
            this.PMOTargetButton.Name = "PMOTargetButton";
            this.PMOTargetButton.Size = new System.Drawing.Size(164, 38);
            this.PMOTargetButton.TabIndex = 2;
            this.PMOTargetButton.Text = "Choose Target PMO";
            this.PMOTargetButton.UseVisualStyleBackColor = true;
            this.PMOTargetButton.Click += new System.EventHandler(this.PMOTargetButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 65);
            this.Controls.Add(this.PMOTargetButton);
            this.Controls.Add(this.ApplyRetargettingButton);
            this.Controls.Add(this.PMOSourceButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.Text = "PMO Retargetting Tool";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button PMOSourceButton;
        private System.Windows.Forms.Button ApplyRetargettingButton;
        private System.Windows.Forms.Button PMOTargetButton;
    }
}

