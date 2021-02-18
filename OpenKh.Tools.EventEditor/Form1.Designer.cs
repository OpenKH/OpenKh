
namespace OpenKh.Tools.EventEditor
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LoadEventButton = new System.Windows.Forms.Button();
            this.SaveEventButton = new System.Windows.Forms.Button();
            this.FlowEvent = new System.Windows.Forms.FlowLayoutPanel();
            this.AddEventButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LoadEventButton
            // 
            this.LoadEventButton.Font = new System.Drawing.Font("Segoe UI Historic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.LoadEventButton.Location = new System.Drawing.Point(13, 13);
            this.LoadEventButton.Name = "LoadEventButton";
            this.LoadEventButton.Size = new System.Drawing.Size(75, 23);
            this.LoadEventButton.TabIndex = 0;
            this.LoadEventButton.Text = "Load Event";
            this.LoadEventButton.UseVisualStyleBackColor = true;
            this.LoadEventButton.Click += new System.EventHandler(this.LoadEventButton_Click);
            // 
            // SaveEventButton
            // 
            this.SaveEventButton.Enabled = false;
            this.SaveEventButton.Font = new System.Drawing.Font("Segoe UI Historic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.SaveEventButton.Location = new System.Drawing.Point(94, 13);
            this.SaveEventButton.Name = "SaveEventButton";
            this.SaveEventButton.Size = new System.Drawing.Size(75, 23);
            this.SaveEventButton.TabIndex = 1;
            this.SaveEventButton.Text = "Save as...";
            this.SaveEventButton.UseVisualStyleBackColor = true;
            this.SaveEventButton.Click += new System.EventHandler(this.SaveEventButton_Click);
            // 
            // FlowEvent
            // 
            this.FlowEvent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FlowEvent.AutoScroll = true;
            this.FlowEvent.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.FlowEvent.Location = new System.Drawing.Point(13, 43);
            this.FlowEvent.Name = "FlowEvent";
            this.FlowEvent.Size = new System.Drawing.Size(652, 395);
            this.FlowEvent.TabIndex = 2;
            // 
            // AddEventButton
            // 
            this.AddEventButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddEventButton.Enabled = false;
            this.AddEventButton.Location = new System.Drawing.Point(576, 12);
            this.AddEventButton.Name = "AddEventButton";
            this.AddEventButton.Size = new System.Drawing.Size(88, 23);
            this.AddEventButton.TabIndex = 3;
            this.AddEventButton.Text = "Add event";
            this.AddEventButton.UseVisualStyleBackColor = true;
            this.AddEventButton.Click += new System.EventHandler(this.AddEventButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(676, 450);
            this.Controls.Add(this.AddEventButton);
            this.Controls.Add(this.FlowEvent);
            this.Controls.Add(this.SaveEventButton);
            this.Controls.Add(this.LoadEventButton);
            this.Name = "Form1";
            this.Text = "Event Editor";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button LoadEventButton;
        private System.Windows.Forms.Button SaveEventButton;
        private System.Windows.Forms.FlowLayoutPanel FlowEvent;
        private System.Windows.Forms.Button AddEventButton;
    }
}

