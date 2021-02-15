
namespace OpenKh.Tools.EpdEditor
{
    partial class ExtraControl
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
            this.ExtraParamGBox = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ParameterValue = new System.Windows.Forms.TextBox();
            this.ParameterName = new System.Windows.Forms.TextBox();
            this.ExtraParamGBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ExtraParamGBox
            // 
            this.ExtraParamGBox.Controls.Add(this.label2);
            this.ExtraParamGBox.Controls.Add(this.label1);
            this.ExtraParamGBox.Controls.Add(this.ParameterValue);
            this.ExtraParamGBox.Controls.Add(this.ParameterName);
            this.ExtraParamGBox.Location = new System.Drawing.Point(5, 5);
            this.ExtraParamGBox.Name = "ExtraParamGBox";
            this.ExtraParamGBox.Size = new System.Drawing.Size(211, 71);
            this.ExtraParamGBox.TabIndex = 0;
            this.ExtraParamGBox.TabStop = false;
            this.ExtraParamGBox.Text = "Extra Param 1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(107, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Value";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Name";
            // 
            // ParameterValue
            // 
            this.ParameterValue.Location = new System.Drawing.Point(110, 38);
            this.ParameterValue.Name = "ParameterValue";
            this.ParameterValue.Size = new System.Drawing.Size(87, 23);
            this.ParameterValue.TabIndex = 1;
            // 
            // ParameterName
            // 
            this.ParameterName.Location = new System.Drawing.Point(6, 38);
            this.ParameterName.Name = "ParameterName";
            this.ParameterName.Size = new System.Drawing.Size(98, 23);
            this.ParameterName.TabIndex = 0;
            // 
            // ExtraControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ExtraParamGBox);
            this.Name = "ExtraControl";
            this.Size = new System.Drawing.Size(227, 84);
            this.ExtraParamGBox.ResumeLayout(false);
            this.ExtraParamGBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox ExtraParamGBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox ParameterValue;
        public System.Windows.Forms.TextBox ParameterName;
    }
}
