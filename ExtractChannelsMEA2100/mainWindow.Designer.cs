namespace ExtractChannels2
{
    partial class MainWindow
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lstFiles2 = new System.Windows.Forms.ListView();
            this.hdrFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hdrStimulusID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hdrStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnUp = new System.Windows.Forms.Button();
            this.layoutActionButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnExtractSelected = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.txtCurrentfile = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.txtLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.barProgress = new System.Windows.Forms.ToolStripProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.layoutActionButtons.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lstFiles2);
            this.splitContainer1.Panel1.Controls.Add(this.flowLayoutPanel1);
            this.splitContainer1.Panel1.Controls.Add(this.layoutActionButtons);
            this.splitContainer1.Panel1MinSize = 100;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtOutput);
            this.splitContainer1.Panel2.Controls.Add(this.txtCurrentfile);
            this.splitContainer1.Size = new System.Drawing.Size(1007, 423);
            this.splitContainer1.SplitterDistance = 232;
            this.splitContainer1.TabIndex = 6;
            // 
            // lstFiles2
            // 
            this.lstFiles2.CheckBoxes = true;
            this.lstFiles2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.hdrFile,
            this.hdrStimulusID,
            this.hdrStatus});
            this.lstFiles2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstFiles2.FullRowSelect = true;
            this.lstFiles2.Location = new System.Drawing.Point(0, 0);
            this.lstFiles2.Name = "lstFiles2";
            this.lstFiles2.Size = new System.Drawing.Size(926, 203);
            this.lstFiles2.TabIndex = 12;
            this.lstFiles2.UseCompatibleStateImageBehavior = false;
            this.lstFiles2.View = System.Windows.Forms.View.Details;
            // 
            // hdrFile
            // 
            this.hdrFile.Text = "File";
            this.hdrFile.Width = 750;
            // 
            // hdrStimulusID
            // 
            this.hdrStimulusID.Text = "StimulusID";
            this.hdrStimulusID.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.hdrStimulusID.Width = 70;
            // 
            // hdrStatus
            // 
            this.hdrStatus.Text = "Status";
            this.hdrStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.hdrStatus.Width = 70;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.btnDown);
            this.flowLayoutPanel1.Controls.Add(this.btnUp);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(926, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(81, 203);
            this.flowLayoutPanel1.TabIndex = 11;
            this.flowLayoutPanel1.Visible = false;
            // 
            // btnDown
            // 
            this.btnDown.Location = new System.Drawing.Point(3, 3);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(75, 23);
            this.btnDown.TabIndex = 1;
            this.btnDown.Text = "Down";
            this.btnDown.UseVisualStyleBackColor = true;
            // 
            // btnUp
            // 
            this.btnUp.Location = new System.Drawing.Point(3, 32);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(75, 23);
            this.btnUp.TabIndex = 0;
            this.btnUp.Text = "Up";
            this.btnUp.UseVisualStyleBackColor = true;
            // 
            // layoutActionButtons
            // 
            this.layoutActionButtons.AutoSize = true;
            this.layoutActionButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.layoutActionButtons.Controls.Add(this.btnClear);
            this.layoutActionButtons.Controls.Add(this.btnExtractSelected);
            this.layoutActionButtons.Controls.Add(this.btnLoad);
            this.layoutActionButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.layoutActionButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.layoutActionButtons.Location = new System.Drawing.Point(0, 203);
            this.layoutActionButtons.Name = "layoutActionButtons";
            this.layoutActionButtons.Size = new System.Drawing.Size(1007, 29);
            this.layoutActionButtons.TabIndex = 11;
            // 
            // btnClear
            // 
            this.btnClear.AutoSize = true;
            this.btnClear.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnClear.Location = new System.Drawing.Point(963, 3);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(41, 23);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnExtractSelected
            // 
            this.btnExtractSelected.AutoSize = true;
            this.btnExtractSelected.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnExtractSelected.Location = new System.Drawing.Point(864, 3);
            this.btnExtractSelected.Name = "btnExtractSelected";
            this.btnExtractSelected.Size = new System.Drawing.Size(93, 23);
            this.btnExtractSelected.TabIndex = 1;
            this.btnExtractSelected.Text = "Extract selected";
            this.btnExtractSelected.UseVisualStyleBackColor = true;
            this.btnExtractSelected.Click += new System.EventHandler(this.ButtonClick_ExtractBins);
            // 
            // btnLoad
            // 
            this.btnLoad.AutoSize = true;
            this.btnLoad.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnLoad.Location = new System.Drawing.Point(779, 3);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(79, 23);
            this.btnLoad.TabIndex = 11;
            this.btnLoad.Text = "Load folder...";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // txtOutput
            // 
            this.txtOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtOutput.Location = new System.Drawing.Point(0, 0);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(1007, 167);
            this.txtOutput.TabIndex = 3;
            // 
            // txtCurrentfile
            // 
            this.txtCurrentfile.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.txtCurrentfile.Location = new System.Drawing.Point(0, 167);
            this.txtCurrentfile.Name = "txtCurrentfile";
            this.txtCurrentfile.ReadOnly = true;
            this.txtCurrentfile.Size = new System.Drawing.Size(1007, 20);
            this.txtCurrentfile.TabIndex = 10;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.txtLabel,
            this.barProgress});
            this.statusStrip1.Location = new System.Drawing.Point(0, 423);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1007, 22);
            this.statusStrip1.TabIndex = 8;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // txtLabel
            // 
            this.txtLabel.Name = "txtLabel";
            this.txtLabel.Size = new System.Drawing.Size(992, 17);
            this.txtLabel.Spring = true;
            this.txtLabel.Text = "Ready";
            this.txtLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // barProgress
            // 
            this.barProgress.Name = "barProgress";
            this.barProgress.Size = new System.Drawing.Size(100, 16);
            this.barProgress.Visible = false;
            // 
            // MainWindow
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1007, 445);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.MinimumSize = new System.Drawing.Size(400, 39);
            this.Name = "MainWindow";
            this.Text = "ExtractChannels MEA2100";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.layoutActionButtons.ResumeLayout(false);
            this.layoutActionButtons.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Button btnExtractSelected;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel txtLabel;
        private System.Windows.Forms.ToolStripProgressBar barProgress;
        private System.Windows.Forms.TextBox txtCurrentfile;
        private System.Windows.Forms.FlowLayoutPanel layoutActionButtons;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.ListView lstFiles2;
        private System.Windows.Forms.ColumnHeader hdrFile;
        private System.Windows.Forms.ColumnHeader hdrStimulusID;
        private System.Windows.Forms.ColumnHeader hdrStatus;
    }
}

