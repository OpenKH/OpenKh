
namespace OpenKh.Tools.OloEditor
{
    partial class ObjectNameControl
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
            this.ObjectNameText = new System.Windows.Forms.TextBox();
            this.GBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // GBox
            // 
            this.GBox.Controls.Add(this.ObjectNameText);
            this.GBox.Location = new System.Drawing.Point(4, 0);
            this.GBox.Name = "GBox";
            this.GBox.Size = new System.Drawing.Size(144, 56);
            this.GBox.TabIndex = 0;
            this.GBox.TabStop = false;
            this.GBox.Text = "Object Name 1";
            // 
            // ObjectNameText
            // 
            this.ObjectNameText.Location = new System.Drawing.Point(6, 22);
            this.ObjectNameText.MaxLength = 15;
            this.ObjectNameText.Name = "ObjectNameText";
            this.ObjectNameText.Size = new System.Drawing.Size(124, 23);
            this.ObjectNameText.TabIndex = 0;
            // 
            // ObjectNameControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.GBox);
            this.Name = "ObjectNameControl";
            this.Size = new System.Drawing.Size(155, 61);
            this.GBox.ResumeLayout(false);
            this.GBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox GBox;
        public System.Windows.Forms.TextBox ObjectNameText;
    }
}
