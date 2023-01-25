
namespace OpenKh.Tools.BepEditor
{
    partial class DisappearParameters
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
            this.DisappearGBox = new System.Windows.Forms.GroupBox();
            this.DistanceTextBox = new System.Windows.Forms.TextBox();
            this.NumericRoomID = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.WorldIDComboBox = new System.Windows.Forms.ComboBox();
            this.DisappearGBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericRoomID)).BeginInit();
            this.SuspendLayout();
            // 
            // DisappearGBox
            // 
            this.DisappearGBox.Controls.Add(this.WorldIDComboBox);
            this.DisappearGBox.Controls.Add(this.DistanceTextBox);
            this.DisappearGBox.Controls.Add(this.NumericRoomID);
            this.DisappearGBox.Controls.Add(this.label3);
            this.DisappearGBox.Controls.Add(this.label2);
            this.DisappearGBox.Controls.Add(this.label1);
            this.DisappearGBox.Location = new System.Drawing.Point(4, 4);
            this.DisappearGBox.Name = "DisappearGBox";
            this.DisappearGBox.Size = new System.Drawing.Size(306, 83);
            this.DisappearGBox.TabIndex = 0;
            this.DisappearGBox.TabStop = false;
            this.DisappearGBox.Text = "Disappear 1";
            // 
            // DistanceTextBox
            // 
            this.DistanceTextBox.Location = new System.Drawing.Point(193, 41);
            this.DistanceTextBox.Name = "DistanceTextBox";
            this.DistanceTextBox.Size = new System.Drawing.Size(100, 23);
            this.DistanceTextBox.TabIndex = 5;
            this.DistanceTextBox.Text = "1";
            // 
            // NumericRoomID
            // 
            this.NumericRoomID.Location = new System.Drawing.Point(134, 42);
            this.NumericRoomID.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.NumericRoomID.Name = "NumericRoomID";
            this.NumericRoomID.Size = new System.Drawing.Size(53, 23);
            this.NumericRoomID.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(193, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "Distance";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(134, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Room ID";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "World ID";
            // 
            // WorldIDComboBox
            // 
            this.WorldIDComboBox.FormattingEnabled = true;
            this.WorldIDComboBox.Location = new System.Drawing.Point(7, 42);
            this.WorldIDComboBox.Name = "WorldIDComboBox";
            this.WorldIDComboBox.Size = new System.Drawing.Size(121, 23);
            this.WorldIDComboBox.TabIndex = 6;
            this.WorldIDComboBox.Text = "Land of Departure";
            // 
            // DisappearParameters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DisappearGBox);
            this.Name = "DisappearParameters";
            this.Size = new System.Drawing.Size(325, 94);
            this.DisappearGBox.ResumeLayout(false);
            this.DisappearGBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericRoomID)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox DisappearGBox;
        public System.Windows.Forms.TextBox DistanceTextBox;
        public System.Windows.Forms.NumericUpDown NumericRoomID;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.ComboBox WorldIDComboBox;
    }
}
