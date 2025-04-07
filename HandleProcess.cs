using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AutoClick_Zefoy
{
    public static class HandleProcess
    {
        private static int totalViews = 0; // Total view count
        private static bool _faSearchChecked = false; // Biến cờ để kiểm tra chuỗi "Please wait X minute(s) Y second(s) before trying again"
        private static System.Windows.Forms.Timer countdownTimer; // Timer to handle countdown
        private static int remainingTime; // Remaining time for countdown

        public static void HandleFaSearch(string? clipboardText, Random rnd, Action<string> updateStatus, Action<string> updateNextClick, Label statusLabel, Label totalTimeLabel, WebContentHandler siteStatus, Dictionary<string, Point> clickPoints)
        {
            if (!string.IsNullOrEmpty(clipboardText) && !_faSearchChecked)
            {
                _faSearchChecked = true; // Đánh dấu đã kiểm tra chuỗi "Please wait X minute(s) Y second(s) before trying again"

                // Handle different status messages
                if (clipboardText.Contains("Please wait"))
                {
                    int waitMinutes = ParseWaitTime(clipboardText, "minute(s)");
                    int waitSeconds = ParseWaitTime(clipboardText, "second(s)");
                    int totalWaitTime = (waitMinutes * 60) + waitSeconds;

                    // Display total wait time on GUI
                    siteStatus.UpdateStatus($"Please wait {waitMinutes} minute(s) {waitSeconds} second(s) before trying again.");
                    StartCountdown(totalTimeLabel, totalWaitTime, $"Tổng thời gian chờ còn: {totalWaitTime} giây để click fa-search");

                    // Wait for the specified time
                    Thread.Sleep(totalWaitTime * 1000);
                    // Click fa-search after waiting
                    if (clickPoints.TryGetValue("fa-search", out Point faSearchPoint))
                    {
                        ClickAt(faSearchPoint);
                        updateNextClick($"Tọa độ sắp click: {GetNextKey("fa-search", clickPoints)} (X={faSearchPoint.X}, Y={faSearchPoint.Y})");
                        Thread.Sleep(rnd.Next(1000, 2000)); // Wait for a random duration between 1 to 2 seconds
                    }
                }
                else if (clipboardText.Contains("Next Submit: READY....!"))
                {
                    siteStatus.UpdateStatus("Next Submit: READY....!");
                    siteStatus.DisplayStatus(statusLabel, totalTimeLabel);
                }
                else if (clipboardText.Contains("Successfully"))
                {
                    siteStatus.UpdateStatus(clipboardText);
                    siteStatus.DisplayStatus(statusLabel, totalTimeLabel);

                    // Extract the number of views from the success message
                    int viewsSent = ParseViewsSent(clipboardText);
                    totalViews += viewsSent; // Cumulate the views
                    updateStatus?.Invoke($"Tổng số view: {totalViews}"); // Ensure updateStatus is not null

                    // Stop the countdown timer after successfully clicking fa-video-camera
                    ResetCountdown();
                }
                else if (clipboardText.Contains("This service is currently not working."))
                {
                    siteStatus.UpdateStatus("This service is currently not working.");
                    siteStatus.DisplayStatus(statusLabel, totalTimeLabel);
                }
                else if (clipboardText.Contains("Please try again later or refresh the page use other services."))
                {
                    siteStatus.UpdateStatus("Please try again later or refresh the page use other services.");
                    siteStatus.DisplayStatus(statusLabel, totalTimeLabel);
                }
                else
                {
                    if (clickPoints.TryGetValue("fa-video-camera", out Point faVideoCameraPoint))
                    {
                        ClickAt(faVideoCameraPoint);
                        updateNextClick($"Tọa độ sắp click: {GetNextKey("fa-video-camera", clickPoints)} (X={faVideoCameraPoint.X}, Y={faVideoCameraPoint.Y})");
                        Thread.Sleep(rnd.Next(1000, 2000)); // Wait for a random duration between 1 to 2 seconds

                        // Stop the countdown timer for fa-video-camera
                        ResetCountdown();
                    }
                }
            }
        }

        public static void HandleFormControl(string currentUrl, Point nextClick, Random rnd, Action<string> updateStatus, Action<string> updateCurrentUrl, Action<string> updateNextClick, Label statusLabel, Label totalTimeLabel, InputHandler inputHandler, Dictionary<string, Point> clickPoints, WebContentHandler siteStatus)
        {
            updateStatus($"URL hiện tại: {currentUrl}");
            updateCurrentUrl($"URL sắp click: {currentUrl} ({clickPoints.Count})");

            if (clickPoints.TryGetValue("fa-search", out Point faSearchPoint))
            {
                ClickAt(faSearchPoint); // Move to the fa-search coordinate
                updateNextClick($"Tọa độ sắp click: {GetNextKey("fa-search", clickPoints)} (X={faSearchPoint.X}, Y={faSearchPoint.Y})");
                Thread.Sleep(rnd.Next(2000, 3000)); // Wait for a random duration between 2 to 3 seconds

                // Read and process clipboard content
                string clipboardText = inputHandler.ReadClipboard();
                if (!string.IsNullOrEmpty(clipboardText))
                {
                    // Handle 'Please wait' message only once
                    if (clipboardText.Contains("Please wait"))
                    {
                        HandleFaSearch(clipboardText, rnd, updateStatus, updateNextClick, statusLabel, totalTimeLabel, siteStatus, clickPoints);
                    }
                    else
                    {
                        // Normal click transition
                        updateNextClick($"Tọa độ sắp click: {GetNextKey("fa-search", clickPoints)} (X={faSearchPoint.X}, Y={faSearchPoint.Y})");
                        Thread.Sleep(rnd.Next(1000, 2000)); // Wait for a random duration between 1 to 2 seconds
                    }
                }
            }
        }

        public static void HandleFaVideoCamera(Point nextClick, Random rnd, Action<string> updateStatus, Action<string> updateNextClick, Label statusLabel, Label totalTimeLabel, WebContentHandler siteStatus, Dictionary<string, Point> clickPoints)
        {
            ClickAt(nextClick);
            updateNextClick?.Invoke($"Tọa độ sắp click: {GetNextKey("fa-video-camera", clickPoints)} (X={nextClick.X}, Y={nextClick.Y})");
            Thread.Sleep(rnd.Next(3000, 5000)); // Wait for a random duration between 3 to 5 seconds

            // Check for success message
            string windowText = GetZefoyWindowText(new InputHandler(new AutoClickManager(null, new Label(), s => { })));
            if (!string.IsNullOrEmpty(windowText) && windowText.Contains("Successfully"))
            {
                siteStatus?.UpdateStatus(windowText);
                siteStatus?.DisplayStatus(statusLabel, totalTimeLabel);

                // Extract the number of views from the success message
                int viewsSent = ParseViewsSent(windowText);
                totalViews += viewsSent; // Cumulate the views
                updateStatus?.Invoke($"Tổng số view: {totalViews}"); // Ensure updateStatus is not null

                // Stop the countdown timer after successfully clicking fa-video-camera
                ResetCountdown();
            }
            else
            {
                siteStatus?.UpdateStatus("Next Submit: READY....!");
                siteStatus?.DisplayStatus(statusLabel, totalTimeLabel);
            }
        }

        public static string ProcessClipboard(Point nextClick, string key, Random rnd, string currentUrl, Action<string> updateNextClick, InputHandler inputHandler, Dictionary<string, Point> clickPoints)
        {
            ClickAt(nextClick); // Click to focus
            Thread.Sleep(500); // Wait for focus
            SendKeys.SendWait("^a");  // Ctrl + A to select all
            Thread.Sleep(500); // Wait for the selection
            SendKeys.SendWait("^c");  // Ctrl + C to copy
            Thread.Sleep(500); // Wait for the copy to complete

            // Now the clipboard contains the copied content and can be processed
            string clipboardText = inputHandler.ReadClipboard();  // Reading the clipboard content

            if (key == "form-control" && !string.IsNullOrWhiteSpace(currentUrl))
            {
                SendKeys.SendWait("{DEL}");  // Delete the selected text
                Thread.Sleep(500); // Wait for the deletion
                SendKeys.SendWait(currentUrl);
                Thread.Sleep(500); // Wait for the URL to be entered
                ClickAt(nextClick); // Click again to ensure focus
                updateNextClick($"Tọa độ sắp click: {GetNextKey(key, clickPoints)} (X={nextClick.X}, Y={nextClick.Y})");
                Thread.Sleep(500); // Wait for focus
                SendKeys.SendWait("{ENTER}");
                Thread.Sleep(rnd.Next(3000, 5000)); // Wait for a random duration between 3 to 5 seconds
            }

            return clipboardText ?? string.Empty;  // Ensure non-null string
        }

        private static void ClickAt(Point pos)
        {
            SetCursorPos(pos.X, pos.Y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, pos.X, pos.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, pos.X, pos.Y, 0, 0);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
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

        private static string GetNextKey(string currentKey, Dictionary<string, Point> clickPoints)
        {
            List<string> clickSequence = new List<string> { "fa-arrow-right", "form-control", "fa-search", "fa-video-camera" };
            int currentIndex = clickSequence.IndexOf(currentKey);
            int nextIndex = (currentIndex + 1) % clickSequence.Count;
            return clickSequence[nextIndex];
        }

        private static string GetZefoyWindowText(InputHandler inputHandler)
        {
            IntPtr browserHwnd = FindWindow("Chrome_WidgetWin_1", null); // Assuming using Chrome
            if (browserHwnd == IntPtr.Zero)
            {
                string? input = inputHandler.ShowInputDialog("Enter the window name (e.g., Zefoy):");
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

        private static int ParseWaitTime(string text, string timeUnit)
        {
            int time = 0;
            int index = text.IndexOf(timeUnit);
            if (index > 0)
            {
                string timeString = text.Substring(0, index).Trim();
                string[] parts = timeString.Split(' ');
                if (parts.Length > 0 && int.TryParse(parts[parts.Length - 1], out int parsedTime))
                {
                    time = parsedTime;
                }
            }
            return time;
        }

        private static int ParseViewsSent(string text)
        {
            int views = 0;
            string marker = "Successfully";
            int index = text.IndexOf(marker);
            if (index >= 0)
            {
                string viewsString = text.Substring(index + marker.Length).Trim().Split(' ')[0];
                int.TryParse(viewsString, out views);
            }
            return views;
        }

        private static void StartCountdown(Label totalTimeLabel, int totalWaitTime, string message)
        {
            countdownTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            remainingTime = totalWaitTime;

            countdownTimer.Tick += (sender, args) =>
            {
                if (remainingTime > 0)
                {
                    remainingTime--;
                    int minutes = remainingTime / 60;
                    int seconds = remainingTime % 60;
                    totalTimeLabel.Invoke((MethodInvoker)(() => totalTimeLabel.Text = $"{message}: {remainingTime} giây"));
                }
                else
                {
                    countdownTimer.Stop();
                }
            };
            countdownTimer.Start();
        }

        private static void ResetCountdown()
        {
            if (countdownTimer != null)
            {
                countdownTimer.Stop();
                remainingTime = 0;
            }
        }
    }
}