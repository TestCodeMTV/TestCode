using System;
using System.Collections.Generic;
using System.Diagnostics; // Added for Process reference
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace AutoClick_Zefoy
{
    public class AutoClickManager
    {
        private Dictionary<string, Point> clickPoints = new();
        private List<string> urlList = new();
        private bool isClicking;
        private SiteStatus siteStatus = new SiteStatus();

        public int UrlCount => urlList.Count;
        public bool IsClicking => isClicking;

        public void LoadClickPoints(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    Converters = { new PointJsonConverter() }
                };
                clickPoints = JsonSerializer.Deserialize<Dictionary<string, Point>>(json, options) ?? new Dictionary<string, Point>();
            }
        }

        public void SaveClickPoints(string filePath)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new PointJsonConverter() },
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(clickPoints, options);
            File.WriteAllText(filePath, json);
        }

        public void ImportUrls(string filePath)
        {
            urlList = new List<string>(File.ReadAllLines(filePath));
        }

        public void StartAutoClick(int minViews, int maxViews, Action<string> updateStatus, Action<string> updateNextClick,
            Action<string> updateCurrentUrl, Action<int, int> updateViewsIncreased, Label statusLabel, Label totalTimeLabel)
        {
            isClicking = true;
            new Thread(() => AutoClickLoop(minViews, maxViews, updateStatus, updateNextClick, updateCurrentUrl, updateViewsIncreased, statusLabel, totalTimeLabel)).Start();
        }

        public void StopAutoClick()
        {
            isClicking = false;
        }

        private void AutoClickLoop(int minViews, int maxViews, Action<string> updateStatus, Action<string> updateNextClick,
            Action<string> updateCurrentUrl, Action<int, int> updateViewsIncreased, Label statusLabel, Label totalTimeLabel)
        {
            Random rnd = new Random();
            int viewsIncreased = 0;
            int loopCount = urlList.Count;
            int urlIndex = 0;

            for (int i = 0; i < loopCount && isClicking; i++)
            {
                if (urlIndex >= urlList.Count)
                {
                    break;
                }
                string currentUrl = urlList[urlIndex];
                int clickCount = rnd.Next(minViews, maxViews + 1);
                updateViewsIncreased(0, clickCount);

                for (int j = 0; j < clickCount; j++)
                {
                    PerformAutoClick(currentUrl, rnd, updateStatus, updateNextClick, updateCurrentUrl, statusLabel, totalTimeLabel);
                    viewsIncreased++;
                    updateViewsIncreased(viewsIncreased, clickCount);
                }

                urlIndex = (urlIndex + 1) % urlList.Count;
                viewsIncreased = 0;  // Reset for the next URL
            }
        }

        public Dictionary<string, Point> GetClickPoints()
        {
            return clickPoints;
        }

        private void PerformAutoClick(string currentUrl, Random rnd, Action<string> updateStatus, Action<string> updateNextClick,
            Action<string> updateCurrentUrl, Label statusLabel, Label totalTimeLabel)
        {
            foreach (var key in clickPoints.Keys)
            {
                Point nextClick = clickPoints[key];
                updateNextClick($"Tọa độ sắp click: {key} (X={nextClick.X}, Y={nextClick.Y})");

                if (key == "form-control")
                {
                    updateStatus($"URL hiện tại: {currentUrl}");
                    updateCurrentUrl($"URL đang click: {currentUrl} ({urlList.IndexOf(currentUrl) + 1})");
                    if (!string.IsNullOrWhiteSpace(currentUrl))
                    {
                        ClickAt(nextClick); // Click to focus
                        Thread.Sleep(500); // Wait for focus
                        SendKeys.SendWait("^a");  // Select all text
                        Thread.Sleep(500); // Wait for the selection
                        SendKeys.SendWait("{DEL}");  // Delete the selected text
                        Thread.Sleep(500); // Wait for the deletion
                        SendKeys.SendWait(currentUrl);
                        Thread.Sleep(500); // Wait for the URL to be entered
                        ClickAt(nextClick); // Click again to ensure focus
                        Thread.Sleep(500); // Wait for focus
                        SendKeys.SendWait("{ENTER}");
                        Thread.Sleep(rnd.Next(3000, 5000)); // Wait for a random duration between 3 to 5 seconds
                        if (clickPoints.ContainsKey("fa-search"))
                        {
                            ClickAt(clickPoints["fa-search"]); // Move to the fa-search coordinate
                            Thread.Sleep(rnd.Next(30000, 35000)); // Wait for a random duration between 30 to 35 seconds

                            // Check for wait message
                            string windowText = GetZefoyWindowText();
                            if (!string.IsNullOrEmpty(windowText))
                            {
                                siteStatus.UpdateStatus(windowText);
                                siteStatus.DisplayStatus(statusLabel, totalTimeLabel);
                            }
                        }
                    }
                }
                else if (key == "fa-search")
                {
                    ClickAt(nextClick);
                    Thread.Sleep(rnd.Next(30000, 35000)); // Wait for a random duration between 30 to 35 seconds

                    // Check for wait message
                    string windowText = GetZefoyWindowText();
                    if (!string.IsNullOrEmpty(windowText))
                    {
                        siteStatus.UpdateStatus(windowText);
                        siteStatus.DisplayStatus(statusLabel, totalTimeLabel);
                    }
                }
                else
                {
                    ClickAt(nextClick);
                    Thread.Sleep(rnd.Next(10000, 20000));
                }
            }
        }

        private void ClickAt(Point pos)
        {
            SetCursorPos(pos.X, pos.Y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, pos.X, pos.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, pos.X, pos.Y, 0, 0);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);

        private delegate bool EnumChildProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern void SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        private string GetZefoyWindowText()
        {
            IntPtr browserHwnd = FindWindow("Chrome_WidgetWin_1", null); // Assuming using Chrome
            if (browserHwnd == IntPtr.Zero)
            {
                string input = ShowInputDialog("Enter the window name (e.g., Zefoy):");
                if (string.IsNullOrWhiteSpace(input))
                {
                    return string.Empty;  // Ensure non-null string
                }

                browserHwnd = FindWindow(null, input);
                if (browserHwnd == IntPtr.Zero)
                {
                    MessageBox.Show("Could not find the specified window.");
                    return string.Empty;  // Ensure non-null string
                }
            }

            StringBuilder windowText = new StringBuilder(1024);
            EnumChildWindows(browserHwnd, (hwnd, lParam) =>
            {
                GetWindowText(hwnd, windowText, windowText.Capacity);
                if (windowText.ToString().Contains("Zefoy"))
                {
                    return false; // Stop enumeration
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);

            return windowText.ToString();
        }

        private string ShowInputDialog(string promptText)
        {
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

            return prompt.ShowDialog() == DialogResult.OK ? inputBox.Text : string.Empty;
        }
    }
}