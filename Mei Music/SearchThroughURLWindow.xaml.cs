using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Mei_Music
{
    /// <summary>
    /// Interaction logic for SearchThroughURLWindow.xaml
    /// </summary>
    public partial class SearchThroughURLWindow : Window
    {
        MainWindow mainWindow;
        public SearchThroughURLWindow(MainWindow main_Window)
        {
            InitializeComponent();
            mainWindow = main_Window;
        }

        private void RemovePlaceholderText(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "")
            {
                PlaceholderText.Visibility = Visibility.Collapsed;
            }
        }
        private void AddPlaceholderText(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == "")
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string videoUrl = SearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(videoUrl))
            {
                MessageBox.Show("Please enter a valid URL.");
                return;
            }

            string customFileName = PromptForFileName();
            if (string.IsNullOrEmpty(customFileName))
            {
                MessageBox.Show("File name cannot be empty.");
                return;
            }

            // Set the path for the downloaded video using the custom file name
            string? videoFilePath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Mei Music",
                "temp",
                "video",
                customFileName + ".mp4");

            videoFilePath = DownloadVideo(videoUrl, videoFilePath);
            if (string.IsNullOrEmpty(videoFilePath))
            {
                return;
            }

            string audioFilePath = ConvertVideoToAudio(videoFilePath);
            if (audioFilePath != null)
            {
                mainWindow.AddFileToUI(audioFilePath);
            }
        }
        private string? DownloadVideo(string videoUrl, string videoFilePath) // Download video from URL
        {
            try
            {
                string ytDlpPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "yt-dlp", "yt-dlp.exe");

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ytDlpPath,
                    // -o specifies the output file path. We enclose the path in quotes for safe command-line usage.
                    Arguments = $"-o \"{videoFilePath}\" \"{videoUrl}\"",
                    RedirectStandardOutput = true, // Capture processing info
                    RedirectStandardError = true,  // Capture processing errors
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Start the yt-dlp process
                Process? process = Process.Start(startInfo);
                if (process == null)
                {
                    MessageBox.Show("Failed to start the yt-dlp process.");
                    return null;
                }

                using (process)
                {
                    // Capture the output and error information (optional, for debugging purposes)
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        MessageBox.Show($"yt-dlp process failed with error: {error}");
                        return null;
                    }
                }

                return videoFilePath; // Return the path of the downloaded video file
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to download video: {ex.Message}");
                return null;
            }
        }
        private string ConvertVideoToAudio(string videoFilePath) //perform conversion from video to audio
        {
            try
            {
                string outputDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mei Music", "playlist");
                Directory.CreateDirectory(outputDirectory); // Ensure the directory structure exists

                string audioFilePath = System.IO.Path.Combine(outputDirectory, System.IO.Path.GetFileNameWithoutExtension(videoFilePath) + ".mp3");

                string ffmpegPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ffmpeg", "ffmpeg.exe");

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    //q:a 0 set audio quality to best. map a allows extraction of only audio -y allows to overwrite output files
                    Arguments = $"-i \"{videoFilePath}\" -q:a 0 -map a \"{audioFilePath}\" -y",
                    RedirectStandardOutput = true, //capture processing info
                    RedirectStandardError = true,  //capture processing error
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                Process? process = Process.Start(startInfo); //the ? indicate that process may be null
                if (process == null)
                {
                    throw new InvalidOperationException("Fail to start the ffmpeg process.");
                }
                using (process)
                {
                    process.WaitForExit();
                }
                return audioFilePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to convert Video: {ex.Message}");
                throw;
            }
        }
        private string PromptForFileName()
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter a name for the downloaded file (without extension):",
                "File Name",
                "MyVideo"); // Default name is "MyVideo"

            // Sanitize the input by removing invalid characters
            string sanitizedFileName = string.Join("_", input.Split(System.IO.Path.GetInvalidFileNameChars()));

            return sanitizedFileName;
        }

        //------------------------- Icon bar implementation --------------------------------
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //----------------------------------------------------------------------------------
    }
}
