using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AutoClick_Zefoy
{
    public class SiteStatus
    {
        public string Status { get; private set; } = string.Empty;  // Khởi tạo giá trị mặc định
        public int TotalTimeInSeconds { get; private set; }

        public void UpdateStatus(string windowText)
        {
            var match = Regex.Match(windowText, @"Please wait (\d+) minute\(s\) (\d+) second\(s\) before trying again");
            if (match.Success)
            {
                int minutes = int.Parse(match.Groups[1].Value);
                int seconds = int.Parse(match.Groups[2].Value);
                TotalTimeInSeconds = (minutes * 60) + seconds;
                Status = $"Please wait {minutes} minute(s) {seconds} second(s) before trying again";
            }
            else
            {
                Status = "No wait message detected.";
                TotalTimeInSeconds = 0;
            }
        }

        public void DisplayStatus(Label statusLabel, Label totalTimeLabel)
        {
            statusLabel.Invoke((MethodInvoker)(() => statusLabel.Text = Status));
            totalTimeLabel.Invoke((MethodInvoker)(() => totalTimeLabel.Text = $"Tổng thời gian: {TotalTimeInSeconds} giây"));
        }
    }
}