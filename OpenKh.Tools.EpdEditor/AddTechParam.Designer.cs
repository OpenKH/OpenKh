
namespace OpenKh.Tools.EpdEditor
{
    partial class AddTechParam
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
            this.AddTechControlButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // AddTechControlButton
            // 
            this.AddTechControlButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AddTechControlButton.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.AddTechControlButton.Location = new System.Drawing.Point(14, 15);
            this.AddTechControlButton.Name = "AddTechControlButton";
            this.AddTechControlButton.Size = new System.Drawing.Size(90, 90);
            this.AddTechControlButton.TabIndex = 0;
            this.AddTechControlButton.Text = "+";
            this.AddTechControlButton.UseVisualStyleBackColor = true;
            this.AddTechControlButton.Click += new System.EventHandler(this.AddTechControlButton_Click);
            // 
            // AddTechParam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.AddTechControlButton);
            this.Name = "AddTechParam";
            this.Size = new System.Drawing.Size(118, 118);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button AddTechControlButton;
    }
}
