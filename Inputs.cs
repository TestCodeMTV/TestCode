using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace AutoClick_Zefoy
{
    public class Inputs
    {
        private readonly Dictionary<Keys, string> keyMapping = new()
        {
            { Keys.F5, "fa-arrow-right" },
            { Keys.F6, "form-control" },
            { Keys.F7, "fa-search" },
            { Keys.F8, "fa-video-camera" }
        };

        private readonly Dictionary<string, Point> clickPoints = new();

        public void HookMouseEvents(Label lblMousePosition)
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 100; // Update every 100 milliseconds
            timer.Tick += (sender, args) =>
            {
                var position = Cursor.Position;
                lblMousePosition.Invoke((MethodInvoker)(() =>
                {
                    lblMousePosition.Text = $"Tọa độ chuột: X: {position.X}, Y: {position.Y}";
                }));
            };
            timer.Start();
        }

        public void HandleKeyDown(KeyEventArgs e, Label lblGuide)
        {
            if (keyMapping.ContainsKey(e.KeyCode))
            {
                var position = Cursor.Position;
                string keyName = keyMapping[e.KeyCode];

                clickPoints[keyName] = position;
                UpdateGuide(lblGuide);
                MessageBox.Show($"Đã lưu {keyName} tại tọa độ: X={position.X}, Y={position.Y}", "Ghi tọa độ tạm thời. Bấm 'Lưu tọa độ' để lưu vào file.");
            }
        }

        private void UpdateGuide(Label lblGuide)
        {
            lblGuide.Invoke((MethodInvoker)(() =>
            {
                lblGuide.Text = "Hướng dẫn:\n" +
                                $"Nhấn F5 để lưu tọa độ 'fa-arrow-right' ({GetPointString("fa-arrow-right")})\n" +
                                $"Nhấn F6 để lưu tọa độ 'form-control' ({GetPointString("form-control")})\n" +
                                $"Nhấn F7 để lưu tọa độ 'fa-search' ({GetPointString("fa-search")})\n" +
                                $"Nhấn F8 để lưu tọa độ 'fa-video-camera' ({GetPointString("fa-video-camera")})";
            }));
        }

        private string GetPointString(string key)
        {
            return clickPoints.ContainsKey(key) ? $"X={clickPoints[key].X}, Y={clickPoints[key].Y}" : "Chưa lưu";
        }
    }
}