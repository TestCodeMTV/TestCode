using System;
using System.IO;
using System.Windows.Forms;

namespace AutoClick_Zefoy
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                File.AppendAllText("log.txt", $"{DateTime.Now}: Exception in Main: {ex}{Environment.NewLine}");
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}