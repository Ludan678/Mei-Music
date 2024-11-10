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
            ProcessingProgressBar.Visibility = Visibility.Visible;
            ProcessingText.Visibility = Visibility.Visible;

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

            // Define paths for both downloaded files and final output
            string downloadedDirectory = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Mei Music",
                "temp",
                "downloaded");

            string finalVideoDirectory = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Mei Music",
                "temp",
                "video");

            Directory.CreateDirectory(downloadedDirectory);
            Directory.CreateDirectory(finalVideoDirectory);
            Directory.GetFiles(downloadedDirectory).ToList().ForEach(File.Delete);

            string finalVideoPath = System.IO.Path.Combine(finalVideoDirectory, customFileName + ".mp4");

          
            DownloadVideo(videoUrl, downloadedDirectory);
            
           
            // Check downloaded files
            var downloadedFiles = Directory.GetFiles(downloadedDirectory);
            var downloadedVideoPath = downloadedFiles.FirstOrDefault(f => f.EndsWith(".mp4"));
            var downloadedAudioPath = downloadedFiles.FirstOrDefault(f => f.EndsWith(".m4a"));
            
            // Verify if the files were downloaded correctly
            if (downloadedVideoPath == null)
            {
                MessageBox.Show("Video file was not downloaded.");
                return;
            }

            if (downloadedAudioPath != null)
            {
                // Step 3: Convert m4a to aac for compatibility
                string aacAudioPath = ConvertM4aToAac(downloadedAudioPath);
                if (aacAudioPath != null)
                {
                    // Step 4: Combine the video and converted audio
                    CombineVideoAndAudio(downloadedVideoPath, aacAudioPath, finalVideoPath);
                }
                else
                {
                    MessageBox.Show("AAC conversion failed.");
                }
            }
            else
            {
                // Case 1: Only the video file exists, so use it as the final output
                File.Copy(downloadedVideoPath, finalVideoPath, overwrite: true);
            }

            // Clean up downloaded files in temp/downloaded
            Directory.GetFiles(downloadedDirectory).ToList().ForEach(File.Delete);

            if (File.Exists(finalVideoPath))
            {
                // Add the final combined video to the UI
                mainWindow.AddFileToUI(finalVideoPath);
                ConvertVideoToAudio(finalVideoPath);
            }

            ProcessingProgressBar.Visibility = Visibility.Collapsed;
            ProcessingText.Visibility = Visibility.Collapsed;
        }
        private void DownloadVideo(string videoUrl, string downloadDirectory) // Download video from URL
        {
            try
            {
                string ytDlpPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "yt-dlp", "yt-dlp.exe");

                // Sanitize the filename to avoid invalid characters
                string sanitizedFileName = SanitizeFileName("video"); // Basic name to ensure fallback
                string videoFilePath = System.IO.Path.Combine(downloadDirectory, $"{sanitizedFileName}.mp4");

                // Define the arguments based on the site URL
                string arguments;
                if (videoUrl.StartsWith("https://www.bilibili.com/"))
                {
                    // For Bilibili: Download best video and audio separately, but avoid playlists
                    arguments = $"--no-playlist --format \"bestvideo+bestaudio/best\" -o \"{System.IO.Path.Combine(downloadDirectory, "%(title)s.%(ext)s")}\" \"{videoUrl}\"";
                }
                else
                {
                    // For all other sites: Download best single format and avoid playlists
                    arguments = $"--no-playlist --format best -o \"{videoFilePath}\" \"{videoUrl}\"";
                }

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ytDlpPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true, // Capture processing info
                    RedirectStandardError = true,  // Capture processing errors
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process? process = Process.Start(startInfo))
                {
                    process.WaitForExit(); // Synchronously wait for process to complete

                    string error = process.StandardError.ReadToEnd();
                    if (process.ExitCode != 0)
                    {
                        MessageBox.Show($"Download failed with error: {error}");
                        return;
                    }
                }

                var downloadedFiles = Directory.GetFiles(downloadDirectory);
                if (downloadedFiles.Length > 2)
                {
                    foreach (var file in downloadedFiles)
                    {
                        if (!file.EndsWith(".mp4") && !file.EndsWith(".m4a"))
                        {
                            File.Delete(file); // Delete non-mp4 and non-m4a files
                        }
                    }
                }

                downloadedFiles = Directory.GetFiles(downloadDirectory, "*.mp4").Concat(Directory.GetFiles(downloadDirectory, "*.m4a")).ToArray();
                if (downloadedFiles.Length == 0 || !downloadedFiles.Any(f => f.EndsWith(".mp4")))
                {
                    MessageBox.Show("No video was downloaded. The URL may be invalid or the video is not available.");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during download: {ex.Message}");
            }
        }
        private async Task MonitorDirectoryForFileLimit(string directory, Process process, int maxFiles)
        {
            try
            {
                while (!process.HasExited)
                {
                    var files = Directory.GetFiles(directory);
                    if (files.Length > maxFiles)
                    {
                        // Kill the download process if there are more than maxFiles in the directory
                        process.Kill();
                        MessageBox.Show("Exceeded file limit; terminating download.");
                        break;
                    }
                    await Task.Delay(500); // Check every 500 ms
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while monitoring files: {ex.Message}");
            }
        }
        private string SanitizeFileName(string fileName)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_'); // Replace invalid characters with underscore
            }
            return fileName;
        }
        private void CombineVideoAndAudio(string videoPath, string audioPath, string outputPath)
        {
            string ffmpegPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ffmpeg", "ffmpeg.exe");

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a copy -strict experimental \"{outputPath}\" -y",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process? process = Process.Start(startInfo))
            {
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    MessageBox.Show($"FFmpeg failed to combine video and audio: {error}");
                }
            }
        }
        private string ConvertM4aToAac(string audioPath)
        {
            try
            {
                
                // Ensure the output file has a different name by appending "_converted" to avoid conflicts
                string outputDirectory = System.IO.Path.GetDirectoryName(audioPath) ?? "";
                string aacAudioPath = System.IO.Path.Combine(outputDirectory, System.IO.Path.GetFileNameWithoutExtension(audioPath) + "_converted.aac");

                string ffmpegPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ffmpeg", "ffmpeg.exe");

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = $"-i \"{audioPath}\" -c:a aac \"{aacAudioPath}\" -y",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process? process = Process.Start(startInfo))
                {
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    // Check if the FFmpeg command completed successfully
                    if (process.ExitCode != 0)
                    {
                        MessageBox.Show($"FFmpeg failed to convert m4a to aac: {error}");
                        return null;
                    }
                }

                // Confirm if the output file is created
                if (File.Exists(aacAudioPath))
                {
                    return aacAudioPath;
                }
                else
                {
                    MessageBox.Show("AAC file was not created.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to convert m4a to aac: {ex.Message}");
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
