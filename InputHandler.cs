using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace AutoClick_Zefoy
{
    public class InputHandler
    {
        private readonly Dictionary<Keys, string> keyMapping = new()
        {
            { Keys.F5, "fa-arrow-right" },
            { Keys.F6, "form-control" },
            { Keys.F7, "fa-search" },
            { Keys.F8, "fa-video-camera" }
        };

        private readonly AutoClickManager autoClickManager;

        public InputHandler(AutoClickManager manager)
        {
            autoClickManager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public void HookMouseEvents(Label lblMousePosition)
        {
            if (lblMousePosition == null) throw new ArgumentNullException(nameof(lblMousePosition));

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
            if (e == null) throw new ArgumentNullException(nameof(e));
            if (lblGuide == null) throw new ArgumentNullException(nameof(lblGuide));

            if (keyMapping.ContainsKey(e.KeyCode))
            {
                var position = Cursor.Position;
                string keyName = keyMapping[e.KeyCode];

                autoClickManager.SetClickPoint(keyName, position);
                UpdateGuide(lblGuide);
                MessageBox.Show($"Đã lưu {keyName} tại tọa độ: X={position.X}, Y={position.Y}", "Ghi tọa độ tạm thời. Bấm 'Lưu tọa độ' để lưu vào file.");
            }
        }

        private void UpdateGuide(Label lblGuide)
        {
            if (lblGuide == null) throw new ArgumentNullException(nameof(lblGuide));

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
            var clickPoints = autoClickManager.GetClickPoints();
            return clickPoints.ContainsKey(key) ? $"X={clickPoints[key].X}, Y={clickPoints[key].Y}" : "Chưa lưu";
        }

        public string ReadClipboard()
        {
            string clipboardText = string.Empty;
            Thread staThread = new Thread(
                delegate ()
                {
                    try
                    {
                        clipboardText = Clipboard.GetText();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error reading clipboard: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return clipboardText ?? string.Empty; // Ensure non-null string
        }

        public string ShowInputDialog(string promptText)
        {
            if (promptText == null) throw new ArgumentNullException(nameof(promptText));

            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Input",
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = promptText };
            TextBox inputBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(inputBox);
            prompt.Controls.Add(confirmation);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? inputBox.Text ?? string.Empty : string.Empty; // Ensure non-null string
        }

        public void UpdateCoordinatesOnGUI(Label lblMousePosition)
        {
            if (lblMousePosition == null) throw new ArgumentNullException(nameof(lblMousePosition));

            var clickPoints = autoClickManager.GetClickPoints();
            lblMousePosition.Invoke((MethodInvoker)(() =>
            {
                lblMousePosition.Text = $"Tọa độ chuột: X: {Cursor.Position.X}, Y: {Cursor.Position.Y}";
            }));
        }

        public void ImportUrls(string[] urls)
        {
            if (urls == null) throw new ArgumentNullException(nameof(urls));

            autoClickManager.ImportUrls(urls);
        }

        public void Dispose()
        {
            // Add any necessary cleanup code here
        }
    }
}