
namespace OpenKh.Tools.OloEditor
{
    partial class ObjectLoadedControl
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
            this.ObjectLoadedComboBox = new System.Windows.Forms.ComboBox();
            this.GBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // GBox
            // 
            this.GBox.Controls.Add(this.ObjectLoadedComboBox);
            this.GBox.Location = new System.Drawing.Point(4, 0);
            this.GBox.Name = "GBox";
            this.GBox.Size = new System.Drawing.Size(217, 58);
            this.GBox.TabIndex = 0;
            this.GBox.TabStop = false;
            this.GBox.Text = "Object Loaded 1";
            // 
            // ObjectLoadedComboBox
            // 
            this.ObjectLoadedComboBox.FormattingEnabled = true;
            this.ObjectLoadedComboBox.Location = new System.Drawing.Point(6, 22);
            this.ObjectLoadedComboBox.Name = "ObjectLoadedComboBox";
            this.ObjectLoadedComboBox.Size = new System.Drawing.Size(200, 23);
            this.ObjectLoadedComboBox.TabIndex = 0;
            // 
            // ObjectLoadedControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.GBox);
            this.Name = "ObjectLoadedControl";
            this.Size = new System.Drawing.Size(229, 63);
            this.GBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox GBox;
        public System.Windows.Forms.ComboBox ObjectLoadedComboBox;
    }
}
