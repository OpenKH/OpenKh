
namespace OpenKh.Tools.OloEditor
{
    partial class PathNameControl
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
            this.GBox = new System.Windows.Forms.GroupBox();
            this.PathNameText = new System.Windows.Forms.TextBox();
            this.GBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // GBox
            // 
            this.GBox.Controls.Add(this.PathNameText);
            this.GBox.Location = new System.Drawing.Point(4, 0);
            this.GBox.Name = "GBox";
            this.GBox.Size = new System.Drawing.Size(184, 57);
            this.GBox.TabIndex = 0;
            this.GBox.TabStop = false;
            this.GBox.Text = "Path Name 1";
            // 
            // PathNameText
            // 
            this.PathNameText.Location = new System.Drawing.Point(6, 22);
            this.PathNameText.MaxLength = 31;
            this.PathNameText.Name = "PathNameText";
            this.PathNameText.Size = new System.Drawing.Size(169, 23);
            this.PathNameText.TabIndex = 0;
            // 
            // PathNameControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.GBox);
            this.Name = "PathNameControl";
            this.Size = new System.Drawing.Size(194, 63);
            this.GBox.ResumeLayout(false);
            this.GBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox GBox;
        public System.Windows.Forms.TextBox PathNameText;
    }
}
