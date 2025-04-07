using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AutoClick_Zefoy
{
    public class Inputs : IDisposable
    {
        private readonly AutoClickManager autoClickManager;
        private System.Windows.Forms.Timer? mousePositionTimer;
        private Label mousePositionLabel;

        public Inputs(AutoClickManager manager)
        {
            autoClickManager = manager;
            mousePositionLabel = new Label(); // Initialize with a default value
        }

        public void HookMouseEvents(Label mousePositionLabel)
        {
            this.mousePositionLabel = mousePositionLabel;

            // Initialize and start mouse position timer
            mousePositionTimer = new System.Windows.Forms.Timer();
            mousePositionTimer.Interval = 100; // 0.1 second
            mousePositionTimer.Tick += MousePositionTimer_Tick;
            mousePositionTimer.Start();
        }

        public void HandleKeyDown(KeyEventArgs e, Label guideLabel)
        {
            Point mousePosition = Cursor.Position;
            string key = e.KeyCode.ToString();
            string clickPointKey = key switch
            {
                "F5" => "fa-arrow-right",
                "F6" => "form-control",
                "F7" => "fa-search",
                "F8" => "fa-video-camera",
                _ => null
            };

            if (clickPointKey != null)
            {
                autoClickManager.SetClickPoint(clickPointKey, mousePosition);
                guideLabel.Text += $"\nLưu tọa độ {clickPointKey}: X={mousePosition.X}, Y={mousePosition.Y}";
            }
        }

        private void MousePositionTimer_Tick(object? sender, EventArgs e)
        {
            Point mousePosition = Cursor.Position;
            mousePositionLabel.Text = $"Tọa độ Chuột\n{mousePosition.X}, {mousePosition.Y}";
        }

        public void UpdateCoordinatesOnGUI(Label lblMousePosition)
        {
            try
            {
                var clickPoints = autoClickManager.GetClickPoints();
                lblMousePosition.Text = "Tọa độ Chuột\n" + string.Join(Environment.NewLine, clickPoints.Select(kvp => $"{kvp.Value.X}, {kvp.Value.Y}"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating coordinates on GUI: " + ex.Message);
                MessageBox.Show("An error occurred while updating coordinates on GUI: " + ex.Message, "Error");
            }
        }

        public void ImportUrls(string[] urls)
        {
            try
            {
                autoClickManager.ImportUrls(urls);
                MessageBox.Show("URLs đã được nhập thành công!", "Import URLs", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error importing URLs: " + ex.Message);
                MessageBox.Show("An error occurred while importing URLs: " + ex.Message, "Error");
            }
        }

        public void Dispose()
        {
            mousePositionTimer?.Dispose();
        }
    }
}