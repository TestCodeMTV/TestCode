using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AutoClick_Zefoy
{
    public partial class MainForm : Form
    {
        private readonly AutoClickManager autoClickManager;
        private readonly InputHandler inputHandler;
        private System.Windows.Forms.Timer countdownTimer;
        private TimeSpan remainingTime;
        private int clickCounter = 0;

        private const string FILE_PATH = "click_sequence.json";

        public MainForm()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);

            // Initialize autoClickManager and inputHandler
            autoClickManager = new AutoClickManager(null, lblTotalTime, UpdateNextClick);
            inputHandler = new InputHandler(autoClickManager);

            countdownTimer = new System.Windows.Forms.Timer();
            countdownTimer.Interval = 1000; // 1 second
            countdownTimer.Tick += CountdownTimer_Tick;

            lblTotalTime.Text = "Thời gian đếm ngược: 00:00:00"; // Initialize with 0

            try
            {
                if (File.Exists(FILE_PATH))
                {
                    autoClickManager.LoadClickPoints(FILE_PATH);
                    Console.WriteLine($"Loaded click points from {FILE_PATH}");
                    inputHandler.UpdateCoordinatesOnGUI(lblMousePosition); // Update the GUI with loaded coordinates
                }
                else
                {
                    Console.WriteLine($"File {FILE_PATH} does not exist. No click points loaded.");
                }

                inputHandler.HookMouseEvents(lblMousePosition);
                UpdateGuideText();
                UpdateTotalUrls();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during form initialization: " + ex.Message);
                MessageBox.Show("An error occurred during form initialization: " + ex.Message, "Error");
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Initialize or load necessary data when the form loads
        }

        private void UpdateGuideText()
        {
            try
            {
                var clickPoints = autoClickManager.GetClickPoints();
                lblGuide.Text = "Hướng dẫn:\n" +
                                $"Nhấn F5 để lưu tọa độ 'fa-arrow-right' (X={(clickPoints.ContainsKey("fa-arrow-right") ? clickPoints["fa-arrow-right"].X : 0)}, Y={(clickPoints.ContainsKey("fa-arrow-right") ? clickPoints["fa-arrow-right"].Y : 0)})\n" +
                                $"Nhấn F6 để lưu tọa độ 'form-control' (X={(clickPoints.ContainsKey("form-control") ? clickPoints["form-control"].X : 0)}, Y={(clickPoints.ContainsKey("form-control") ? clickPoints["form-control"].Y : 0)})\n" +
                                $"Nhấn F7 để lưu tọa độ 'fa-search' (X={(clickPoints.ContainsKey("fa-search") ? clickPoints["fa-search"].X : 0)}, Y={(clickPoints.ContainsKey("fa-search") ? clickPoints["fa-search"].Y : 0)})\n" +
                                $"Nhấn F8 để lưu tọa độ 'fa-video-camera' (X={(clickPoints.ContainsKey("fa-video-camera") ? clickPoints["fa-video-camera"].X : 0)}, Y={(clickPoints.ContainsKey("fa-video-camera") ? clickPoints["fa-video-camera"].Y : 0)})\n";
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating guide text: " + ex.Message);
                MessageBox.Show("An error occurred while updating guide text: " + ex.Message, "Error");
            }
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            try
            {
                inputHandler.HandleKeyDown(e, lblGuide);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error handling key down event: " + ex.Message);
                MessageBox.Show("An error occurred during key down event: " + ex.Message, "Error");
            }
        }

        private void btnSaveCoordinates_Click(object sender, EventArgs e)
        {
            try
            {
                autoClickManager.SaveClickPoints(FILE_PATH);
                Console.WriteLine($"Saved click points to {FILE_PATH}");
                MessageBox.Show("Tọa độ đã được lưu thành công!", "Lưu tọa độ");
                UpdateGuideText();
                inputHandler.UpdateCoordinatesOnGUI(lblMousePosition); // Update GUI after saving coordinates
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving coordinates: " + ex.Message);
                MessageBox.Show("An error occurred while saving coordinates: " + ex.Message, "Error");
            }
        }

        private void btnAutoClick_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if URLs have been imported
                if (autoClickManager.UrlCount == 0)
                {
                    MessageBox.Show("Vui lòng nhập URL trước khi bắt đầu tự động click.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int minViews = int.Parse(numMinViews.Text);
                int maxViews = int.Parse(numMaxViews.Text);

                autoClickManager.StartAutoClick(minViews, maxViews, UpdateStatus, UpdateCurrentUrl, UpdateViewsIncreased, lblCurrentStatus, lblNextClick, lblTotalTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting auto click: " + ex.Message);
                MessageBox.Show("An error occurred while starting auto click: " + ex.Message, "Error");
            }
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            if (remainingTime.TotalSeconds > 0)
            {
                remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
                lblTotalTime.Text = $"Thời gian đếm ngược: {remainingTime:hh\\:mm\\:ss}";
            }
            else
            {
                countdownTimer.Stop();
                lblTotalTime.Text = "Thời gian đã hết.";
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                autoClickManager.RequestStopAutoClick();
                countdownTimer.Stop();
                // Do not release resources immediately
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error stopping auto click: " + ex.Message);
                MessageBox.Show("An error occurred while stopping auto click: " + ex.Message, "Error");
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            try
            {
                if (autoClickManager.IsClicking)
                {
                    autoClickManager.RequestStopAutoClick();
                    countdownTimer.Stop();
                }

                // Ensure all resources are freed
                ReleaseResources();
                Application.Exit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error exiting application: " + ex.Message);
                MessageBox.Show("An error occurred while exiting application: " + ex.Message, "Error");
            }
        }

        private void btnLoadCoordinates_Click(object sender, EventArgs e)
        {
            try
            {
                autoClickManager.LoadClickPoints(FILE_PATH);
                Console.WriteLine($"Loaded click points from {FILE_PATH}");
                MessageBox.Show("Tọa độ đã được tải thành công!", "Tải tọa độ");
                UpdateGuideText();
                inputHandler.UpdateCoordinatesOnGUI(lblMousePosition); // Update the GUI with loaded coordinates
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading coordinates: " + ex.Message);
                MessageBox.Show("An error occurred while loading coordinates: " + ex.Message, "Error");
            }
        }

        private void btnImportUrls_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Text Files|*.txt|All Files|*.*",
                    Title = "Import URLs"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string[] urls = File.ReadAllLines(openFileDialog.FileName);
                    inputHandler.ImportUrls(urls);
                    UpdateTotalUrls();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error importing URLs: " + ex.Message);
                MessageBox.Show("An error occurred while importing URLs: " + ex.Message, "Error");
            }
        }

        private void UpdateStatus(string status)
        {
            lblCurrentStatus.Invoke((MethodInvoker)(() => lblCurrentStatus.Text = status));
        }

        private void UpdateNextClick(string nextClick)
        {
            lblNextClick.Invoke((MethodInvoker)(() => lblNextClick.Text = nextClick));
        }

        private void UpdateCurrentUrl(string currentUrl)
        {
            lblCurrentUrl.Invoke((MethodInvoker)(() => lblCurrentUrl.Text = currentUrl));
            UpdateNextUrl(currentUrl);
        }

        private void UpdateViewsIncreased(int viewsIncreased, int totalViews)
        {
            lblViewsIncreased.Invoke((MethodInvoker)(() => lblViewsIncreased.Text = $"Đã tăng: {viewsIncreased}/{totalViews}"));
            if (CheckSuccessNotification())
            {
                clickCounter++;
                lblViewsIncreased.Text = $"Successfully {clickCounter} views sent";
            }
        }

        private bool CheckSuccessNotification()
        {
            string notificationText = lblViewsIncreased.Text;
            Regex regex = new Regex(@"Successfully (\d+) views sent");
            Match match = regex.Match(notificationText);
            if (match.Success)
            {
                int viewsSent = int.Parse(match.Groups[1].Value);
                return viewsSent > 0;
            }
            return false;
        }

        private void lblNextClick_Click(object sender, EventArgs e)
        {
            try
            {
                var currentUrl = lblCurrentUrl.Text;
                var urls = autoClickManager.GetUrls();

                // Find the index of the current URL
                int currentIndex = urls.IndexOf(currentUrl);
                if (currentIndex != -1 && currentIndex < urls.Count - 1)
                {
                    // Set the next URL
                    lblCurrentUrl.Text = urls[currentIndex + 1];
                    lblNextClick.Text = $"Next Submit: READY....!";
                }
                else
                {
                    MessageBox.Show("No more URLs available.", "Info");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting the next URL: " + ex.Message);
                MessageBox.Show("An error occurred while getting the next URL: " + ex.Message, "Error");
            }
        }

        private void lblCurrentUrl_Click(object sender, EventArgs e)
        {
            // Handle lblCurrentUrl click event
        }

        private void numMaxViews_TextChanged(object sender, EventArgs e)
        {
            // Handle numMaxViews text changed event
        }

        private void lblMousePosition_Click(object sender, EventArgs e)
        {
            // Handle lblMousePosition click event
        }

        private void UpdateTotalUrls()
        {
            lblTotalUrls.Text = $"Total URLs: {autoClickManager.UrlCount}";
        }

        private void UpdateNextUrl(string currentUrl)
        {
            var urls = autoClickManager.GetUrls();
            int currentIndex = urls.IndexOf(currentUrl);
            if (urls.Count == 1)
            {
                lblNextClick.Text = $"Next URL: {currentUrl}";
            }
            else if (currentIndex != -1 && currentIndex < urls.Count - 1)
            {
                lblNextClick.Text = $"Next URL: {urls[currentIndex + 1]}";
            }
            else
            {
                lblNextClick.Text = "No more URLs available.";
            }
        }

        private void ReleaseResources()
        {
            try
            {
                countdownTimer.Dispose();
                autoClickManager.Dispose();
                inputHandler.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error releasing resources: " + ex.Message);
                MessageBox.Show("An error occurred while releasing resources: " + ex.Message, "Error");
            }
        }

        private void lblTotalTime_Click(object sender, EventArgs e)
        {
            if (!countdownTimer.Enabled)
            {
                remainingTime = TimeSpan.FromMinutes(5); // Đặt thời gian đếm ngược mong muốn
                lblTotalTime.Text = $"Thời gian đếm ngược: {remainingTime:hh\\:mm\\:ss}";
                countdownTimer.Start(); // Bắt đầu bộ đếm thời gian
            }
        }
    }
}