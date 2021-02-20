
namespace OpenKh.Tools.IteEditor
{
    partial class IteEntry
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
            this.ITE_GBox = new System.Windows.Forms.GroupBox();
            this.ItemComboBox = new System.Windows.Forms.ComboBox();
            this.ITE_GBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ITE_GBox
            // 
            this.ITE_GBox.Controls.Add(this.ItemComboBox);
            this.ITE_GBox.Location = new System.Drawing.Point(4, 0);
            this.ITE_GBox.Name = "ITE_GBox";
            this.ITE_GBox.Size = new System.Drawing.Size(210, 52);
            this.ITE_GBox.TabIndex = 0;
            this.ITE_GBox.TabStop = false;
            this.ITE_GBox.Text = "ITE Entry 1";
            // 
            // ItemComboBox
            // 
            this.ItemComboBox.FormattingEnabled = true;
            this.ItemComboBox.Location = new System.Drawing.Point(6, 22);
            this.ItemComboBox.Name = "ItemComboBox";
            this.ItemComboBox.Size = new System.Drawing.Size(188, 23);
            this.ItemComboBox.TabIndex = 0;
            this.ItemComboBox.Text = "DUMMY";
            // 
            // IteEntry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ITE_GBox);
            this.Name = "IteEntry";
            this.Size = new System.Drawing.Size(220, 60);
            this.ITE_GBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox ITE_GBox;
        public System.Windows.Forms.ComboBox ItemComboBox;
    }
}
