
namespace OpenKh.Tools.ItbEditor
{
    partial class ItbEntry
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
            this.ITB_GBox = new System.Windows.Forms.GroupBox();
            this.NumericReportID = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ItemKindComboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ItemIDComboBox = new System.Windows.Forms.ComboBox();
            this.NumericTreasureBoxID = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.ITB_GBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericReportID)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericTreasureBoxID)).BeginInit();
            this.SuspendLayout();
            // 
            // ITB_GBox
            // 
            this.ITB_GBox.Controls.Add(this.NumericReportID);
            this.ITB_GBox.Controls.Add(this.label4);
            this.ITB_GBox.Controls.Add(this.label3);
            this.ITB_GBox.Controls.Add(this.ItemKindComboBox);
            this.ITB_GBox.Controls.Add(this.label2);
            this.ITB_GBox.Controls.Add(this.ItemIDComboBox);
            this.ITB_GBox.Controls.Add(this.NumericTreasureBoxID);
            this.ITB_GBox.Controls.Add(this.label1);
            this.ITB_GBox.Font = new System.Drawing.Font("Segoe UI Historic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ITB_GBox.Location = new System.Drawing.Point(4, 4);
            this.ITB_GBox.Name = "ITB_GBox";
            this.ITB_GBox.Size = new System.Drawing.Size(562, 78);
            this.ITB_GBox.TabIndex = 0;
            this.ITB_GBox.TabStop = false;
            this.ITB_GBox.Text = "ITB Entry 1";
            // 
            // NumericReportID
            // 
            this.NumericReportID.Location = new System.Drawing.Point(492, 37);
            this.NumericReportID.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.NumericReportID.Name = "NumericReportID";
            this.NumericReportID.Size = new System.Drawing.Size(56, 23);
            this.NumericReportID.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(492, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "Report ID";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(87, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "Kind";
            // 
            // ItemKindComboBox
            // 
            this.ItemKindComboBox.FormattingEnabled = true;
            this.ItemKindComboBox.Items.AddRange(new object[] {
            "ITEM",
            "COMMAND"});
            this.ItemKindComboBox.Location = new System.Drawing.Point(87, 37);
            this.ItemKindComboBox.Name = "ItemKindComboBox";
            this.ItemKindComboBox.Size = new System.Drawing.Size(121, 23);
            this.ItemKindComboBox.TabIndex = 4;
            this.ItemKindComboBox.Text = "ITEM";
            this.ItemKindComboBox.SelectedIndexChanged += new System.EventHandler(this.ItemKindComboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(214, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "Item";
            // 
            // ItemIDComboBox
            // 
            this.ItemIDComboBox.FormattingEnabled = true;
            this.ItemIDComboBox.Items.AddRange(new object[] {
            "Nothing",
            "Wayward Wind = 1",
            "Treasure Trove (Ventus)",
            "Stroke of Midnight (Ventus)",
            "Fairy Stars (Ventus)",
            "Victory Line (Ventus)",
            "Mark of a Hero (Ventus)",
            "Hyperdrive (Ventus)",
            "Pixie Petal (Ventus)",
            "Ultima Weapon (Ventus)",
            "Sweetstack (Ventus)",
            "Light Seeker (Ventus)",
            "it000c (Debug item)",
            "Frolic Flame (Ventus)",
            "Lost Memory (Ventus)",
            "Freeze (?)",
            "Void Gear (Ventus)",
            "No Name (Ventus)",
            "Crown Unlimit (Ventus)",
            "Freeze (?)",
            "Freeze (?)",
            "Freeze (?)",
            "Freeze (?)",
            "Freeze (?)",
            "Earth Shaker (Terra)",
            "Treasure Trove (Terra)",
            "Stroke of Midnight (Terra)",
            "Fairy Stars (Terra)",
            "Victory Line (Terra)",
            "Mark of a Hero (Terra)",
            "Hyperdrive (Terra)",
            "Pixie Petal (Terra)",
            "Ultima Weapon (Terra)",
            "Sweetstack (Terra)",
            "Darkgnaw (Terra)",
            "Ends of Earth (Terra)",
            "Chaos Ripper (Terra)",
            "Freeze (?)",
            "Freeze (?)",
            "Freeze (?)",
            "Void Gear (Terra)",
            "No Name (Terra)",
            "Crown Unlimit (Terra)",
            "Freeze (?)",
            "Rainfell",
            "Treasure Trove (Aqua)",
            "Stroke of Midnight (Aqua)",
            "Fairy Stars (Aqua)",
            "Victory Line (Aqua)",
            "Mark of a Hero (Aqua)",
            "Hyperdrive (Aqua)",
            "Pixie Petal (Aqua)",
            "Ultima Weapon (Aqua)",
            "Sweetstack (Aqua)",
            "Destiny\'s Embrace (Aqua)",
            "Stormfall (Aqua)",
            "Brightcrest (Aqua)",
            "Freeze (?)",
            "Freeze (?)",
            "Freeze (?)",
            "Void Gear (Aqua)",
            "No Name (Aqua)",
            "Crown Unlimit (Aqua)",
            "Master Keeper (Aqua)",
            "Map (Dwarf Woodlands)",
            "Map (Enchanted Dominion)",
            "Map (Radiant Garden)",
            "Map (Disney Town)",
            "Map (Deep Space)",
            "Map (Olympus Coliseum)",
            "Map (Neverland)",
            "Map (Keyblade Graveyard)",
            "Map (Land of Departure)",
            "Map (Castle of Dreams)",
            "Xehanort Letter",
            "Xehanort Report 12",
            "Xehanort Report 1",
            "Xehanort Report 2",
            "Xehanort Report 3",
            "Xehanort Report 4",
            "Xehanort Report 5",
            "Xehanort Report 6",
            "Xehanort Report 7",
            "Xehanort Report 8",
            "Xehanort Report 9",
            "Xehanort Report 10",
            "Xehanort Report 11",
            "Sticker (?)",
            "Mickey Sticker",
            "Minnie Sticker",
            "Huey Sticker",
            "Dewey Sticker",
            "Louie Sticker",
            "Rainbow Sticker",
            "Chip Sticker",
            "Dale Sticker",
            "Fireworks Sticker",
            "Fireworks Sticker",
            "Ice cream Sticker",
            "Ice cream Sticker",
            "Ice cream Sticker",
            "Ice cream Sticker",
            "Ice cream Sticker",
            "Ice cream Sticker",
            "Balloon Sticker",
            "UFO Sticker",
            "Confetti Sticker",
            "Confetti Sticker",
            "????",
            "Pete Sticker",
            "Huey Sticker",
            "Dewey Sticker",
            "Dewey Sticker",
            "Rainbow Sticker",
            "Airplane Sticker",
            "Chip Sticker",
            "Dale Sticker",
            "Flying Balloon Sticker",
            "Flying Balloon Sticker",
            "Bubble Sticker",
            "White Sash",
            "White Button",
            "Pink Fabric",
            "White Lace",
            "Pink Thread",
            "Pearl",
            "Risky Ticket",
            "Sentinel Ticket",
            "Ringer Ticket",
            "Threat Ticket",
            "Treasure Ticket",
            "Chill Ticket",
            "Versus Ticket G",
            "Versus Ticket H",
            "Versus Ticket I",
            "Versus Ticket J",
            "Block Recipe",
            "Action Recipe",
            "Magic Recipe",
            "Mega Magic Recipe",
            "Giga Magic Recipe",
            "Attack Recipe",
            "Mega Attack Recipe",
            "Giga Attack Recipe",
            "Unnamed item (?)",
            "Unnamed item (?)",
            "Star Shard",
            "Disney Town Pass",
            "Wayfinder (Ventus)",
            "Wayfinder (Stitch)",
            "Mirage Pass",
            "Wayfinder (Aqua)",
            "Wayfinder (Terra)",
            "Shimmering Crystal",
            "Shimmering Ore",
            "Fleeting Crystal",
            "Fleeting Ore",
            "Pulsing Crystal",
            "Pulsing Ore",
            "Wellspring Crystal",
            "Wellspring Ore",
            "Soothing Crystal",
            "Soothing Ore",
            "Hungry Crystal",
            "Hungry Ore",
            "Abounding Crystal",
            "Abounding Ore",
            "Chaos Crystal",
            "Secret Gem"});
            this.ItemIDComboBox.Location = new System.Drawing.Point(214, 37);
            this.ItemIDComboBox.Name = "ItemIDComboBox";
            this.ItemIDComboBox.Size = new System.Drawing.Size(272, 23);
            this.ItemIDComboBox.TabIndex = 2;
            this.ItemIDComboBox.Text = "DUMMY";
            this.ItemIDComboBox.SelectedIndexChanged += new System.EventHandler(this.ItemIDComboBox_SelectedIndexChanged);
            // 
            // NumericTreasureBoxID
            // 
            this.NumericTreasureBoxID.Enabled = false;
            this.NumericTreasureBoxID.Location = new System.Drawing.Point(6, 37);
            this.NumericTreasureBoxID.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.NumericTreasureBoxID.Name = "NumericTreasureBoxID";
            this.NumericTreasureBoxID.Size = new System.Drawing.Size(75, 23);
            this.NumericTreasureBoxID.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Treasure ID";
            // 
            // ItbEntry
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ITB_GBox);
            this.Name = "ItbEntry";
            this.Size = new System.Drawing.Size(573, 88);
            this.ITB_GBox.ResumeLayout(false);
            this.ITB_GBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumericReportID)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumericTreasureBoxID)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.GroupBox ITB_GBox;
        public System.Windows.Forms.NumericUpDown NumericTreasureBoxID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.ComboBox ItemIDComboBox;
        public System.Windows.Forms.NumericUpDown NumericReportID;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.ComboBox ItemKindComboBox;
    }
}
