namespace AutoClick_Zefoy
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblGuide;
        private System.Windows.Forms.Label lblMousePosition;
        private System.Windows.Forms.Label lblTotalUrls;
        private System.Windows.Forms.NumericUpDown numMinViews;
        private System.Windows.Forms.NumericUpDown numMaxViews;
        private System.Windows.Forms.Label lblNextClick;
        private System.Windows.Forms.Label lblTotalTime;
        private System.Windows.Forms.Label lblCurrentStatus;
        private System.Windows.Forms.Label lblCurrentUrl;
        private System.Windows.Forms.Label lblViewsIncreased;
        private System.Windows.Forms.Button btnSaveCoordinates;
        private System.Windows.Forms.Button btnAutoClick;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnLoadCoordinates;
        private System.Windows.Forms.Button btnImportUrls;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblGuide = new Label();
            lblMousePosition = new Label();
            lblTotalUrls = new Label();
            numMinViews = new NumericUpDown();
            numMaxViews = new NumericUpDown();
            lblNextClick = new Label();
            lblTotalTime = new Label();
            lblCurrentStatus = new Label();
            lblCurrentUrl = new Label();
            lblViewsIncreased = new Label();
            btnSaveCoordinates = new Button();
            btnAutoClick = new Button();
            btnStop = new Button();
            btnExit = new Button();
            btnLoadCoordinates = new Button();
            btnImportUrls = new Button();
            ((System.ComponentModel.ISupportInitialize)numMinViews).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numMaxViews).BeginInit();
            SuspendLayout();
            // 
            // lblGuide
            // 
            lblGuide.AutoSize = true;
            lblGuide.Location = new Point(138, 10);
            lblGuide.Name = "lblGuide";
            lblGuide.Size = new Size(38, 15);
            lblGuide.TabIndex = 0;
            lblGuide.Text = "Guide";
            // 
            // lblMousePosition
            // 
            lblMousePosition.AutoSize = true;
            lblMousePosition.Location = new Point(13, 12);
            lblMousePosition.Name = "lblMousePosition";
            lblMousePosition.Size = new Size(108, 30);
            lblMousePosition.TabIndex = 1;
            lblMousePosition.Text = "Mouse Coordinate:\nX: 0, Y: 0";
            // 
            // lblTotalUrls
            // 
            lblTotalUrls.AutoSize = true;
            lblTotalUrls.Location = new Point(455, 10);
            lblTotalUrls.Name = "lblTotalUrls";
            lblTotalUrls.Size = new Size(62, 15);
            lblTotalUrls.TabIndex = 2;
            lblTotalUrls.Text = "Total URLs";
            // 
            // numMinViews
            // 
            numMinViews.Location = new Point(584, 35);
            numMinViews.Name = "numMinViews";
            numMinViews.Size = new Size(47, 23);
            numMinViews.TabIndex = 3;
            // 
            // numMaxViews
            // 
            numMaxViews.Location = new Point(647, 35);
            numMaxViews.Name = "numMaxViews";
            numMaxViews.Size = new Size(46, 23);
            numMaxViews.TabIndex = 4;
            // 
            // lblNextClick
            // 
            lblNextClick.AutoSize = true;
            lblNextClick.Location = new Point(138, 148);
            lblNextClick.Name = "lblNextClick";
            lblNextClick.Size = new Size(60, 15);
            lblNextClick.TabIndex = 5;
            lblNextClick.Text = "Next Click";
            // 
            // lblTotalTime
            // 
            lblTotalTime.AutoSize = true;
            lblTotalTime.Location = new Point(138, 176);
            lblTotalTime.Name = "lblTotalTime";
            lblTotalTime.Size = new Size(63, 15);
            lblTotalTime.TabIndex = 6;
            lblTotalTime.Text = "Total Time";
            lblTotalTime.Click += lblTotalTime_Click;
            // 
            // lblCurrentStatus
            // 
            lblCurrentStatus.AutoSize = true;
            lblCurrentStatus.Location = new Point(138, 93);
            lblCurrentStatus.Name = "lblCurrentStatus";
            lblCurrentStatus.Size = new Size(82, 15);
            lblCurrentStatus.TabIndex = 7;
            lblCurrentStatus.Text = "Current Status";
            // 
            // lblCurrentUrl
            // 
            lblCurrentUrl.AutoSize = true;
            lblCurrentUrl.Location = new Point(138, 119);
            lblCurrentUrl.Name = "lblCurrentUrl";
            lblCurrentUrl.Size = new Size(65, 15);
            lblCurrentUrl.TabIndex = 8;
            lblCurrentUrl.Text = "Current Url";
            // 
            // lblViewsIncreased
            // 
            lblViewsIncreased.AutoSize = true;
            lblViewsIncreased.Location = new Point(455, 39);
            lblViewsIncreased.Name = "lblViewsIncreased";
            lblViewsIncreased.Size = new Size(90, 15);
            lblViewsIncreased.TabIndex = 9;
            lblViewsIncreased.Text = "Views Increased";
            // 
            // btnSaveCoordinates
            // 
            btnSaveCoordinates.Location = new Point(12, 128);
            btnSaveCoordinates.Name = "btnSaveCoordinates";
            btnSaveCoordinates.Size = new Size(120, 23);
            btnSaveCoordinates.TabIndex = 10;
            btnSaveCoordinates.Text = "Save Coordinates";
            btnSaveCoordinates.UseVisualStyleBackColor = true;
            btnSaveCoordinates.Click += btnSaveCoordinates_Click;
            // 
            // btnAutoClick
            // 
            btnAutoClick.Location = new Point(573, 6);
            btnAutoClick.Name = "btnAutoClick";
            btnAutoClick.Size = new Size(120, 23);
            btnAutoClick.TabIndex = 12;
            btnAutoClick.Text = "Start Auto Click";
            btnAutoClick.UseVisualStyleBackColor = true;
            btnAutoClick.Click += btnAutoClick_Click;
            // 
            // btnStop
            // 
            btnStop.Location = new Point(709, 6);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(120, 23);
            btnStop.TabIndex = 13;
            btnStop.Text = "Stop Auto Click";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // btnExit
            // 
            btnExit.Location = new Point(709, 35);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(120, 23);
            btnExit.TabIndex = 14;
            btnExit.Text = "Exit";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            // 
            // btnLoadCoordinates
            // 
            btnLoadCoordinates.Location = new Point(12, 168);
            btnLoadCoordinates.Name = "btnLoadCoordinates";
            btnLoadCoordinates.Size = new Size(120, 23);
            btnLoadCoordinates.TabIndex = 15;
            btnLoadCoordinates.Text = "Load Coordinates";
            btnLoadCoordinates.UseVisualStyleBackColor = true;
            btnLoadCoordinates.Click += btnLoadCoordinates_Click;
            // 
            // btnImportUrls
            // 
            btnImportUrls.Location = new Point(13, 68);
            btnImportUrls.Name = "btnImportUrls";
            btnImportUrls.Size = new Size(120, 23);
            btnImportUrls.TabIndex = 16;
            btnImportUrls.Text = "Import URLs";
            btnImportUrls.UseVisualStyleBackColor = true;
            btnImportUrls.Click += btnImportUrls_Click;
            // 
            // MainForm
            // 
            ClientSize = new Size(841, 221);
            Controls.Add(btnImportUrls);
            Controls.Add(btnLoadCoordinates);
            Controls.Add(btnExit);
            Controls.Add(btnStop);
            Controls.Add(btnAutoClick);
            Controls.Add(btnSaveCoordinates);
            Controls.Add(lblViewsIncreased);
            Controls.Add(lblCurrentUrl);
            Controls.Add(lblCurrentStatus);
            Controls.Add(lblTotalTime);
            Controls.Add(lblNextClick);
            Controls.Add(numMaxViews);
            Controls.Add(numMinViews);
            Controls.Add(lblTotalUrls);
            Controls.Add(lblMousePosition);
            Controls.Add(lblGuide);
            Name = "MainForm";
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)numMinViews).EndInit();
            ((System.ComponentModel.ISupportInitialize)numMaxViews).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}