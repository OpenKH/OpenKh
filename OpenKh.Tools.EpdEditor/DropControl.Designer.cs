
namespace OpenKh.Tools.EpdEditor
{
    partial class DropControl
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
            this.DropGBox = new System.Windows.Forms.GroupBox();
            this.NumericItemProbability = new System.Windows.Forms.NumericUpDown();
            this.NumericItemCount = new System.Windows.Forms.NumericUpDown();
            this.ItemComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.DropGBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericItemProbability)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericItemCount)).BeginInit();
            this.SuspendLayout();
            // 
            // DropGBox
            // 
            this.DropGBox.Controls.Add(this.NumericItemProbability);
            this.DropGBox.Controls.Add(this.NumericItemCount);
            this.DropGBox.Controls.Add(this.ItemComboBox);
            this.DropGBox.Controls.Add(this.label3);
            this.DropGBox.Controls.Add(this.label2);
            this.DropGBox.Controls.Add(this.label1);
            this.DropGBox.Location = new System.Drawing.Point(5, 0);
            this.DropGBox.Name = "DropGBox";
            this.DropGBox.Size = new System.Drawing.Size(365, 70);
            this.DropGBox.TabIndex = 0;
            this.DropGBox.TabStop = false;
            this.DropGBox.Text = "Drop Item 1";
            // 
            // NumericItemProbability
            // 
            this.NumericItemProbability.Location = new System.Drawing.Point(280, 37);
            this.NumericItemProbability.Name = "NumericItemProbability";
            this.NumericItemProbability.Size = new System.Drawing.Size(64, 23);
            this.NumericItemProbability.TabIndex = 3;
            // 
            // NumericItemCount
            // 
            this.NumericItemCount.Location = new System.Drawing.Point(199, 37);
            this.NumericItemCount.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.NumericItemCount.Name = "NumericItemCount";
            this.NumericItemCount.Size = new System.Drawing.Size(59, 23);
            this.NumericItemCount.TabIndex = 2;
            this.NumericItemCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // ItemComboBox
            // 
            this.ItemComboBox.FormattingEnabled = true;
            this.ItemComboBox.Items.AddRange(new object[] {
            "ITEM_KIND_HP_SMALL",
            "ITEM_KIND_HP_BIG",
            "ITEM_KIND_MUNNY_SMALL",
            "ITEM_KIND_MUNNY_MIDDEL",
            "ITEM_KIND_MUNNY_BIG",
            "ITEM_KIND_FOCUS_SMALL",
            "ITEM_KIND_FOCUS_BIG",
            "ITEM_KIND_DRAINMIST",
            "ITEM_KIND_D_LINK",
            "ITEM_KIND_MANDORAKE1",
            "ITEM_KIND_MANDORAKE2",
            "ITEM_KIND_JERRYBALL1",
            "ITEM_KIND_JERRYBALL2",
            "ITEM_KIND_JERRYBALL3",
            "ITEM_KIND_JERRYBALL4",
            "ITEM_KIND_JERRYBALL5",
            "ITEM_KIND_JERRYBALL6",
            "ITEM_KIND_JERRYBALL7",
            "ITEM_KIND_JERRYBALL8"});
            this.ItemComboBox.Location = new System.Drawing.Point(7, 38);
            this.ItemComboBox.Name = "ItemComboBox";
            this.ItemComboBox.Size = new System.Drawing.Size(176, 23);
            this.ItemComboBox.TabIndex = 1;
            this.ItemComboBox.Text = "ITEM_KIND_HP_SMALL";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(280, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "Probability";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(199, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Count";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Item ID";
            // 
            // DropControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DropGBox);
            this.Name = "DropControl";
            this.Size = new System.Drawing.Size(377, 75);
            this.DropGBox.ResumeLayout(false);
            this.DropGBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericItemProbability)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericItemCount)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox DropGBox;
        public System.Windows.Forms.NumericUpDown NumericItemProbability;
        public System.Windows.Forms.NumericUpDown NumericItemCount;
        public System.Windows.Forms.ComboBox ItemComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}
