
namespace OpenKh.Tools.EpdEditor
{
    partial class AddExtraParam
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
            this.AddExtraControl = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // AddExtraControl
            // 
            this.AddExtraControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AddExtraControl.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.AddExtraControl.Location = new System.Drawing.Point(7, 8);
            this.AddExtraControl.Name = "AddExtraControl";
            this.AddExtraControl.Size = new System.Drawing.Size(60, 60);
            this.AddExtraControl.TabIndex = 1;
            this.AddExtraControl.Text = "+";
            this.AddExtraControl.UseVisualStyleBackColor = true;
            this.AddExtraControl.Click += new System.EventHandler(this.AddExtraControl_Click);
            // 
            // AddExtraParam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.AddExtraControl);
            this.Name = "AddExtraParam";
            this.Size = new System.Drawing.Size(75, 75);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button AddExtraControl;
    }
}
