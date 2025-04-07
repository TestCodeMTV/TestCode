using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace AutoClick_Zefoy
{
    public class AutoClickManager : IDisposable
    {
        private Dictionary<string, Point> _clickPoints = new();
        private List<string> _urlList = new();
        private bool _isClicking;
        private WebContentHandler _siteStatus = new WebContentHandler();
        private InputHandler _inputHandler;
        private List<string> _clickSequence = new List<string> { "fa-arrow-right", "form-control", "fa-search", "fa-video-camera" };
        private System.Windows.Forms.Timer _countdownTimer;
        private Action<string> _updateNextClick;
        private Label _totalTimeLabel;
        private int _remainingTime;

        public AutoClickManager(InputHandler inputHandler, Label totalTimeLabel, Action<string> updateNextClick)
        {
            _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
            _totalTimeLabel = totalTimeLabel ?? throw new ArgumentNullException(nameof(totalTimeLabel));
            _updateNextClick = updateNextClick ?? throw new ArgumentNullException(nameof(updateNextClick));
            _countdownTimer = new System.Windows.Forms.Timer();
            _countdownTimer.Interval = 1000;
            _countdownTimer.Tick += CountdownTimer_Tick;
        }

        public int UrlCount => _urlList.Count;
        public bool IsClicking => _isClicking;

        public void LoadClickPoints(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions
                {
                    Converters = { new PointJsonConverter() }
                };
                _clickPoints = JsonSerializer.Deserialize<Dictionary<string, Point>>(json, options) ?? new Dictionary<string, Point>();
            }
        }

        public void SaveClickPoints(string filePath)
        {
            var options = new JsonSerializerOptions
            {
                Converters = { new PointJsonConverter() },
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(_clickPoints, options);
            File.WriteAllText(filePath, json);
        }

        public List<string> GetUrls()
        {
            return _urlList;
        }

        public void ImportUrls(string[] urls)
        {
            _urlList.AddRange(urls ?? throw new ArgumentNullException(nameof(urls)));
        }

        public void StartAutoClick(int minViews, int maxViews, Action<string> updateStatus, Action<string> updateCurrentUrl, Action<int, int> updateViewsIncreased, Label statusLabel, Label nextClickLabel, Label totalTimeLabel)
        {
            if (minViews < 0 || maxViews < 0 || minViews > maxViews)
                throw new ArgumentOutOfRangeException("Invalid range for views.");

            if (updateStatus == null || _updateNextClick == null || updateCurrentUrl == null || updateViewsIncreased == null)
                throw new ArgumentNullException("One or more action parameters are null.");

            if (statusLabel == null || _totalTimeLabel == null)
                throw new ArgumentNullException("One or more label parameters are null.");

            _isClicking = true;
            ResetCountdown();
            _totalTimeLabel.Invoke((MethodInvoker)(() => _totalTimeLabel.Text = "Tổng thời gian chờ: 0 phút 0 giây"));
            new Thread(() => AutoClickLoop(minViews, maxViews, updateStatus, updateCurrentUrl, updateViewsIncreased, statusLabel, nextClickLabel, totalTimeLabel)).Start();
        }

        public void StopAutoClick()
        {
            _isClicking = false;
            _countdownTimer.Stop();
        }

        public void SetClickPoint(string key, Point position)
        {
            _clickPoints[key] = position;
        }

        public Dictionary<string, Point> GetClickPoints()
        {
            return _clickPoints;
        }

        private void AutoClickLoop(int minViews, int maxViews, Action<string> updateStatus, Action<string> updateCurrentUrl, Action<int, int> updateViewsIncreased, Label statusLabel, Label nextClickLabel, Label totalTimeLabel)
        {
            Random rnd = new Random();
            int viewsIncreased = 0;
            int loopCount = _urlList.Count;
            int urlIndex = 0;

            while (_isClicking && urlIndex < _urlList.Count)
            {
                string currentUrl = _urlList[urlIndex];
                int clickCount = rnd.Next(minViews, maxViews + 1);
                updateViewsIncreased(0, clickCount);
                updateCurrentUrl($"URL sắp click: {currentUrl}");

                for (int j = 0; j < clickCount && _isClicking; j++)
                {
                    PerformAutoClick(currentUrl, rnd, updateStatus, updateCurrentUrl, updateViewsIncreased, statusLabel, nextClickLabel, totalTimeLabel);
                    viewsIncreased++;
                    updateViewsIncreased(viewsIncreased, clickCount);
                }

                urlIndex++;
                viewsIncreased = 0;

                if (urlIndex >= _urlList.Count)
                {
                    break;
                }
            }

            Dispose();
        }

        private void PerformAutoClick(string currentUrl, Random rnd, Action<string> updateStatus, Action<string> updateCurrentUrl, Action<int, int> updateViewsIncreased, Label statusLabel, Label nextClickLabel, Label totalTimeLabel)
        {
            for (int i = 0; i < _clickSequence.Count && _isClicking; i++)
            {
                string key = _clickSequence[i];
                if (!_clickPoints.TryGetValue(key, out Point nextClick))
                {
                    continue;
                }
                _updateNextClick($"Tọa độ sắp click: {GetNextKey(key)} (X={nextClick.X}, Y={nextClick.Y})");

                string clipboardText = HandleProcess.ProcessClipboard(nextClick, key, rnd, currentUrl, _updateNextClick, _inputHandler, _clickPoints);

                if (key == "fa-search")
                {
                    int waitTime = HandleFaSearchAndGetWaitTime(clipboardText, rnd, updateStatus, statusLabel);
                    if (waitTime > 0)
                    {
                        StartCountdown(waitTime, key);
                    }
                }
                else if (key == "form-control")
                {
                    HandleProcess.HandleFormControl(currentUrl, nextClick, rnd, updateStatus, updateCurrentUrl, _updateNextClick, statusLabel, totalTimeLabel, _inputHandler, _clickPoints, _siteStatus);
                }
                else if (key == "fa-video-camera")
                {
                    HandleProcess.HandleFaVideoCamera(nextClick, rnd, updateStatus, _updateNextClick, statusLabel, totalTimeLabel, _siteStatus, _clickPoints);
                    ResetCountdown();
                }
                else
                {
                    ClickAt(nextClick);
                    _updateNextClick($"Tọa độ sắp click: {GetNextKey(key)} (X={nextClick.X}, Y={nextClick.Y})");
                    int waitTime = rnd.Next(10, 20);
                    StartCountdown(waitTime);
                }
            }
        }

        private string GetNextKey(string currentKey)
        {
            int currentIndex = _clickSequence.IndexOf(currentKey);
            int nextIndex = (currentIndex + 1) % _clickSequence.Count;
            return _clickSequence[nextIndex];
        }

        private void ClickAt(Point pos)
        {
            SetCursorPos(pos.X, pos.Y);
            mouse_event(MOUSEEVENTF_LEFTDOWN, pos.X, pos.Y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, pos.X, pos.Y, 0, 0);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

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

        private string GetZefoyWindowText()
        {
            IntPtr browserHwnd = FindWindow("Chrome_WidgetWin_1", null);
            if (browserHwnd == IntPtr.Zero)
            {
                string? input = _inputHandler.ShowInputDialog("Enter the window name (e.g., Zefoy):");
                if (string.IsNullOrWhiteSpace(input))
                {
                    return string.Empty;
                }

                browserHwnd = FindWindow(null, input);
                if (browserHwnd == IntPtr.Zero)
                {
                    MessageBox.Show("Could not find the specified window.");
                    return string.Empty;
                }
            }

            StringBuilder windowText = new StringBuilder(1024);
            EnumChildWindows(browserHwnd, (hwnd, lParam) =>
            {
                GetWindowText(hwnd, windowText, windowText.Capacity);
                if (windowText.ToString().Contains("Zefoy"))
                {
                    return false;
                }
                return true;
            }, IntPtr.Zero);

            return windowText.ToString();
        }

        private int ParseWaitTime(string text)
        {
            Regex regex = new Regex(@"Please wait (\d+) minute\(s\) (\d+) second\(s\) before trying again");
            Match match = regex.Match(text);
            if (match.Success)
            {
                int minutes = int.Parse(match.Groups[1].Value);
                int seconds = int.Parse(match.Groups[2].Value);
                return minutes * 60 + seconds;
            }
            return 0;
        }

        private void UpdateTotalTimeLabel(int totalWaitTime)
        {
            int minutes = totalWaitTime / 60;
            int seconds = totalWaitTime % 60;
            _totalTimeLabel.Invoke((MethodInvoker)(() => _totalTimeLabel.Text = $"Tổng thời gian chờ: {minutes} phút {seconds} giây"));
        }

        private void StartCountdown(int waitTime, string? currentKey = null)
        {
            _remainingTime = waitTime;
            _countdownTimer.Start();
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            if (_remainingTime > 0)
            {
                _remainingTime--;
                UpdateTotalTimeLabel(_remainingTime);
            }
            else
            {
                _countdownTimer.Stop();
                _totalTimeLabel.Invoke((MethodInvoker)(() => _totalTimeLabel.Text = "Thời gian đã hết"));

                if (_isClicking)
                {
                    Point nextClick = _clickPoints["fa-search"];
                    ClickAt(nextClick);
                    _updateNextClick?.Invoke($"Tọa độ sắp click: {GetNextKey("fa-search")} (X={nextClick.X}, Y={nextClick.Y})");

                    int waitTime = new Random().Next(10, 20);
                    StartCountdown(waitTime, "fa-search");
                }
            }
        }

        private void ResetCountdown()
        {
            _countdownTimer.Stop();
            _totalTimeLabel?.Invoke((MethodInvoker)(() => _totalTimeLabel.Text = "Tổng thời gian chờ: 0 phút 0 giây"));
            _remainingTime = 0;
        }

        private int HandleFaSearchAndGetWaitTime(string clipboardText, Random rnd, Action<string> updateStatus, Label statusLabel)
        {
            string message = clipboardText;
            if (message.Contains("Please wait"))
            {
                int waitTime = ParseWaitTime(message);
                if (waitTime % 120 == 0)
                {
                    message = clipboardText;
                    waitTime = ParseWaitTime(message);
                }
                return waitTime;
            }
            return 0;
        }

        public void Dispose()
        {
            _countdownTimer?.Dispose();
            _isClicking = false;
        }
    }
}