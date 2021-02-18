
namespace OpenKh.Tools.EventEditor
{
    partial class EventEntry
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
            this.EventGBox = new System.Windows.Forms.GroupBox();
            this.NumericGlobalID = new System.Windows.Forms.NumericUpDown();
            this.NumericEventIndex = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.WorldComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.RoomComboBox = new System.Windows.Forms.ComboBox();
            this.EventGBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericGlobalID)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericEventIndex)).BeginInit();
            this.SuspendLayout();
            // 
            // EventGBox
            // 
            this.EventGBox.Controls.Add(this.RoomComboBox);
            this.EventGBox.Controls.Add(this.label4);
            this.EventGBox.Controls.Add(this.label3);
            this.EventGBox.Controls.Add(this.WorldComboBox);
            this.EventGBox.Controls.Add(this.label2);
            this.EventGBox.Controls.Add(this.label1);
            this.EventGBox.Controls.Add(this.NumericEventIndex);
            this.EventGBox.Controls.Add(this.NumericGlobalID);
            this.EventGBox.Location = new System.Drawing.Point(4, 0);
            this.EventGBox.Name = "EventGBox";
            this.EventGBox.Size = new System.Drawing.Size(196, 170);
            this.EventGBox.TabIndex = 0;
            this.EventGBox.TabStop = false;
            this.EventGBox.Text = "Event 1";
            // 
            // NumericGlobalID
            // 
            this.NumericGlobalID.Enabled = false;
            this.NumericGlobalID.Location = new System.Drawing.Point(6, 41);
            this.NumericGlobalID.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.NumericGlobalID.Name = "NumericGlobalID";
            this.NumericGlobalID.Size = new System.Drawing.Size(77, 23);
            this.NumericGlobalID.TabIndex = 0;
            // 
            // NumericEventIndex
            // 
            this.NumericEventIndex.Location = new System.Drawing.Point(119, 41);
            this.NumericEventIndex.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.NumericEventIndex.Name = "NumericEventIndex";
            this.NumericEventIndex.Size = new System.Drawing.Size(71, 23);
            this.NumericEventIndex.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Global ID";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(119, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Event ID";
            // 
            // WorldComboBox
            // 
            this.WorldComboBox.FormattingEnabled = true;
            this.WorldComboBox.Location = new System.Drawing.Point(6, 88);
            this.WorldComboBox.Name = "WorldComboBox";
            this.WorldComboBox.Size = new System.Drawing.Size(184, 23);
            this.WorldComboBox.TabIndex = 4;
            this.WorldComboBox.Text = "Common";
            this.WorldComboBox.SelectedIndexChanged += new System.EventHandler(this.WorldComboBox_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "World";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 114);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(39, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "Room";
            // 
            // RoomComboBox
            // 
            this.RoomComboBox.FormattingEnabled = true;
            this.RoomComboBox.Location = new System.Drawing.Point(6, 132);
            this.RoomComboBox.Name = "RoomComboBox";
            this.RoomComboBox.Size = new System.Drawing.Size(184, 23);
            this.RoomComboBox.TabIndex = 7;
            // 
            // EventEntry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.EventGBox);
            this.Name = "EventEntry";
            this.Size = new System.Drawing.Size(208, 178);
            this.EventGBox.ResumeLayout(false);
            this.EventGBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericGlobalID)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericEventIndex)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox EventGBox;
        public System.Windows.Forms.ComboBox RoomComboBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.ComboBox WorldComboBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.NumericUpDown NumericEventIndex;
        public System.Windows.Forms.NumericUpDown NumericGlobalID;
    }
}
