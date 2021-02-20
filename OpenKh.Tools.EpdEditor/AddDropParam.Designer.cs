
namespace OpenKh.Tools.EpdEditor
{
    partial class AddDropParam
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
            this.AddDropControl = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // AddDropControl
            // 
            this.AddDropControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AddDropControl.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.AddDropControl.Location = new System.Drawing.Point(9, 9);
            this.AddDropControl.Name = "AddDropControl";
            this.AddDropControl.Size = new System.Drawing.Size(60, 55);
            this.AddDropControl.TabIndex = 0;
            this.AddDropControl.Text = "+";
            this.AddDropControl.UseVisualStyleBackColor = true;
            this.AddDropControl.Click += new System.EventHandler(this.AddDropControl_Click);
            // 
            // AddDropParam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.AddDropControl);
            this.Name = "AddDropParam";
            this.Size = new System.Drawing.Size(80, 75);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button AddDropControl;
    }
}
