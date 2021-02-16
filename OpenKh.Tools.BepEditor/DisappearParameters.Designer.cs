
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.NumericWorldID = new System.Windows.Forms.NumericUpDown();
            this.NumericRoomID = new System.Windows.Forms.NumericUpDown();
            this.DistanceTextBox = new System.Windows.Forms.TextBox();
            this.DisappearGBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericWorldID)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericRoomID)).BeginInit();
            this.SuspendLayout();
            // 
            // DisappearGBox
            // 
            this.DisappearGBox.Controls.Add(this.DistanceTextBox);
            this.DisappearGBox.Controls.Add(this.NumericRoomID);
            this.DisappearGBox.Controls.Add(this.NumericWorldID);
            this.DisappearGBox.Controls.Add(this.label3);
            this.DisappearGBox.Controls.Add(this.label2);
            this.DisappearGBox.Controls.Add(this.label1);
            this.DisappearGBox.Location = new System.Drawing.Point(4, 4);
            this.DisappearGBox.Name = "DisappearGBox";
            this.DisappearGBox.Size = new System.Drawing.Size(275, 83);
            this.DisappearGBox.TabIndex = 0;
            this.DisappearGBox.TabStop = false;
            this.DisappearGBox.Text = "Disappear 1";
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(78, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Room ID";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(152, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(52, 15);
            this.label3.TabIndex = 2;
            this.label3.Text = "Distance";
            // 
            // NumericWorldID
            // 
            this.NumericWorldID.Location = new System.Drawing.Point(7, 42);
            this.NumericWorldID.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.NumericWorldID.Name = "NumericWorldID";
            this.NumericWorldID.Size = new System.Drawing.Size(53, 23);
            this.NumericWorldID.TabIndex = 3;
            // 
            // NumericRoomID
            // 
            this.NumericRoomID.Location = new System.Drawing.Point(78, 42);
            this.NumericRoomID.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.NumericRoomID.Name = "NumericRoomID";
            this.NumericRoomID.Size = new System.Drawing.Size(53, 23);
            this.NumericRoomID.TabIndex = 4;
            // 
            // DistanceTextBox
            // 
            this.DistanceTextBox.Location = new System.Drawing.Point(152, 42);
            this.DistanceTextBox.Name = "DistanceTextBox";
            this.DistanceTextBox.Size = new System.Drawing.Size(100, 23);
            this.DistanceTextBox.TabIndex = 5;
            this.DistanceTextBox.Text = "1";
            // 
            // DisappearParameters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DisappearGBox);
            this.Name = "DisappearParameters";
            this.Size = new System.Drawing.Size(288, 94);
            this.DisappearGBox.ResumeLayout(false);
            this.DisappearGBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericWorldID)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericRoomID)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox DisappearGBox;
        public System.Windows.Forms.TextBox DistanceTextBox;
        public System.Windows.Forms.NumericUpDown NumericRoomID;
        public System.Windows.Forms.NumericUpDown NumericWorldID;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}
