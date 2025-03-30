using System;
using System.IO;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutoClick_Zefoy
{
    public partial class MainForm : Form
    {
        private readonly AutoClickManager autoClickManager = new();
        private readonly Inputs inputs = new();
        private const string FILE_PATH = "click_sequence.json";

        public MainForm()
        {
            InitializeComponent();
            try
            {
                if (File.Exists(FILE_PATH))
                {
                    autoClickManager.LoadClickPoints(FILE_PATH);
                    Console.WriteLine($"Loaded click points from {FILE_PATH}");
                }
                else
                {
                    Console.WriteLine($"File {FILE_PATH} does not exist. No click points loaded.");
                }

                inputs.HookMouseEvents(lblMousePosition);
                UpdateGuideText();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during form initialization: " + ex.Message);
                MessageBox.Show("An error occurred during form initialization: " + ex.Message, "Error");
            }
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

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                inputs.HandleKeyDown(e, lblGuide);
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving coordinates: " + ex.Message);
                MessageBox.Show("An error occurred while saving coordinates: " + ex.Message, "Error");
            }
        }

        private void btnImportUrls_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                    openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filePath = openFileDialog.FileName;
                        autoClickManager.ImportUrls(filePath);
                        lblTotalUrls.Text = $"Tổng số URLs: {autoClickManager.UrlCount}";

                        if (autoClickManager.UrlCount > 0)
                        {
                            Console.WriteLine("URLs successfully imported.");
                            MessageBox.Show("URLs đã được nhập thành công!", "Import URLs");
                        }
                        else
                        {
                            Console.WriteLine("File does not contain valid URLs.");
                            MessageBox.Show("File không chứa URL hợp lệ!", "Lỗi");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error importing URLs: " + ex.Message);
                MessageBox.Show("An error occurred while importing URLs: " + ex.Message, "Error");
            }
        }

        private void btnAutoClick_Click(object sender, EventArgs e)
        {
            try
            {
                int minViews = int.Parse(numMinViews.Text);
                int maxViews = int.Parse(numMaxViews.Text);

                autoClickManager.StartAutoClick(minViews, maxViews, UpdateStatus, UpdateNextClick, UpdateCurrentUrl, UpdateViewsIncreased, lblNextClick, lblTotalTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting auto click: " + ex.Message);
                MessageBox.Show("An error occurred while starting auto click: " + ex.Message, "Error");
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                autoClickManager.StopAutoClick();
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
                    autoClickManager.StopAutoClick();
                }
                Application.Exit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error exiting application: " + ex.Message);
                MessageBox.Show("An error occurred while exiting application: " + ex.Message, "Error");
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
        }

        private void UpdateViewsIncreased(int viewsIncreased, int totalViews)
        {
            lblViewsIncreased.Invoke((MethodInvoker)(() => lblViewsIncreased.Text = $"Đã tăng: {viewsIncreased}/{totalViews}"));
        }

        private void GenerateRandomClickCounts()
        {
            try
            {
                Random rnd = new Random();
                int minViews = int.Parse(numMinViews.Text);
                int maxViews = int.Parse(numMaxViews.Text);

                numMinViews.Text = rnd.Next(minViews, maxViews + 1).ToString();
                numMaxViews.Text = rnd.Next(minViews, maxViews + 1).ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error generating random click counts: " + ex.Message);
                MessageBox.Show("An error occurred while generating random click counts: " + ex.Message, "Error");
            }
        }

        private void lblNextClick_Click(object sender, EventArgs e)
        {
            // Handle lblNextClick click event
        }

        private void lblCurrentUrl_Click(object sender, EventArgs e)
        {
            // Handle lblCurrentUrl click event
        }
    }
}