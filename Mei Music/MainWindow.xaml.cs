using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO; 

namespace Mei_Music
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
     
            string audioDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mei Music", "playlist");
            Directory.CreateDirectory(audioDirectory);

            RefreshSongsInUI();
            this.PreviewMouseDown += OnMainWindowPreviewMouseDown; //tracks Position of mouse
        }

        //------------------------- Add Audio Implementation -------------------------------
        private void UploadFromComputer_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Media Files (*.mp3;*.wav;*.mp4,*.mkv;) | *.mp3;*.wav;*.mp4;*.mkv", // description | files shown
                Multiselect = false
            };

            if (ofd.ShowDialog() == true) //if the user chosed a file or 
            {
                string selectedFile = ofd.FileName;
                string fileExtension = System.IO.Path.GetExtension(selectedFile).ToLower(); //allows selection of file from local folder
                string outputDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mei Music", "playlist");
                Directory.CreateDirectory(outputDirectory); // Ensure the directory structure exists

                //is video file
                if (fileExtension == ".mp4" || fileExtension == ".mkv")
                {
                    string audioFilePath = ConvertVideoToAudio(selectedFile); //get the path to audio file
                    if (audioFilePath != null)       
                    {
                        AddFileToUI(audioFilePath);
                    }
                }
                //is audio file
                else if (fileExtension == ".wav" || fileExtension == ".mp3")
                {
                    string audioFilePath = System.IO.Path.Combine(outputDirectory, System.IO.Path.GetFileName(selectedFile));
                    AddFileToUI(selectedFile);
                    File.Copy(selectedFile, audioFilePath, overwrite: true); // Copy file to output directory
                }
            }
        }
        private void SearchThroughURL_Click(object sender, RoutedEventArgs e)
        {

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

        private void AddFileToUI(string filePath)
        {
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath); //get file name

            if (UploadedSongList.Items.Contains(fileNameWithoutExtension)) //if name already exists in the list
            {
                DuplicateDileDialog dialog = new DuplicateDileDialog();
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
                    switch (dialog.SelectedAction)
                    {
                        case DuplicateDileDialog.DuplicateFileAction.Replace:
                            ReplaceFileInUI(filePath);
                            break;

                        case DuplicateDileDialog.DuplicateFileAction.Rename:
                            PromptForNewName(filePath);
                            break;

                        case DuplicateDileDialog.DuplicateFileAction.Cancel:
                            return;
                    }
                }
            }
            else //no duplicate
            {
                UploadedSongList.Items.Add(fileNameWithoutExtension); //add the file name of the audio path to the viewport list
            }
        }

        private void ReplaceFileInUI(string filePath)
        {
            string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath);
            UploadedSongList.Items.Remove(fileNameWithoutExtension);
            UploadedSongList.Items.Add(fileNameWithoutExtension);
        }
        private void PromptForNewName(string filePath)
        {
            string originalName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            string outputDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mei Music", "playlist");
            
            string newName = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter a new name for the file:",
                "Create New Entry",
                 originalName);

            if (!string.IsNullOrEmpty(newName))
            {
                // create file name.mp3
                string newFilePath = System.IO.Path.Combine(outputDirectory, newName + ".mp3");

                // Check if the new name already exists in the directory
                if (File.Exists(newFilePath))
                {
                    MessageBox.Show($"A file named \"{newName}\" already exists in the playlist. Please choose a different name.", "Duplicate Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    // Copy the original file to the new file path with the new name
                    File.Copy(filePath, newFilePath);

                    // Add the new name to the playlist in the UI
                    UploadedSongList.Items.Add(newName);
                }
            }
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            // Open the dropdown and disable the button
            PlusPopupMenu.IsOpen = true;
            PlusButton.IsEnabled = false;
        }
        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            SortPopupMenu.IsOpen = true;
            SortButton.IsEnabled = false;
        }
        //----------------------------------------------------------------------------------



        //-------------------------  Song Functionality Implementation ---------------------
        private void DeleteSong_Click(object sender, RoutedEventArgs e)
        {
           if (sender is Button deleteSong)
            {
                string fileNameWithoutExtension = deleteSong.Tag as string ?? string.Empty; //make sure that audio tag name is not empty
                if (string.IsNullOrEmpty(fileNameWithoutExtension)) return; 

                var dialog = new DeleteSongConfirmationWindow($"Are you sure you want to delete '{fileNameWithoutExtension}'?");
                dialog.Owner = this;
                dialog.ShowDialog();

                if (dialog.IsConfirmed)
                {
                    UploadedSongList.Items.Remove(fileNameWithoutExtension);
                    string audioDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mei Music", "playlist");
                    string audioFile_mp3 = System.IO.Path.Combine(audioDirectory, fileNameWithoutExtension + ".mp3");
                    string audioFile_wav = System.IO.Path.Combine(audioDirectory, fileNameWithoutExtension + ".wav");

                    string? audioFilePath = null;

                    if (File.Exists(audioFile_mp3))
                    {
                        audioFilePath = audioFile_mp3;
                    }
                    if (File.Exists(audioFile_wav))
                    {
                        audioFilePath = audioFile_wav;
                    }

                    if (audioFilePath != null)
                    {
                        File.Delete(audioFilePath);
                    }
                }
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button folderButton)
            {
                // Retrieve the file name (without extension) from the button's Tag
                string fileNameWithoutExtension = folderButton.Tag as string ?? string.Empty;
                if (string.IsNullOrEmpty(fileNameWithoutExtension)) return;

                // Construct the full file path
                string audioDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mei Music", "playlist");
                string audioFile_mp3 = System.IO.Path.Combine(audioDirectory, fileNameWithoutExtension + ".mp3");
                string audioFile_wav = System.IO.Path.Combine(audioDirectory, fileNameWithoutExtension + ".wav");

                string? audioFilePath = null;
               
                if (File.Exists(audioFile_mp3))
                {
                    audioFilePath = audioFile_mp3;
                }
                if (File.Exists(audioFile_wav))
                {
                    audioFilePath = audioFile_wav;
                }

                if (audioFilePath != null) {
                    // Use Process to open the file location with the file selected
                    var psi = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"/select,\"{audioFilePath}\""
                    };
                    Process.Start(psi);
                }
                else
                {
                    MessageBox.Show("File not found in storage.");
                }
            }
        }

        //----------------------------------------------------------------------------------


        //------------------------- Sorting Playlist Implementation ------------------------
        private void SortAlphabetically_Click(object sender, RoutedEventArgs e)
        {
            var sortedItems = UploadedSongList.Items.Cast<string>()
                             .OrderBy(item => item)
                             .ToList();

            RefreshPlaylist(sortedItems);
        }

        private void SortByModification_Click(object sender, RoutedEventArgs e)
        {
            string outputDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mei Music", "playlist");

            var sortedItems = UploadedSongList.Items.Cast<string>()
                             .OrderByDescending(item =>
                             {
                                 string filePath = System.IO.Path.Combine(outputDirectory, item + ".mp3");
                                 return File.GetLastWriteTime(filePath);
                             })
                             .ToList();

            RefreshPlaylist(sortedItems);
        }

        private void RefreshPlaylist(List<string> sortedItems)
        {
            UploadedSongList.Items.Clear();
            foreach (var item in sortedItems)
            {
                UploadedSongList.Items.Add(item);
            }
        }

        private void RefreshSongsInUI()
        {
            // Step 1: Check for any non audio file. Delete them
            string audioDirectory = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mei Music", "playlist");
            bool nonAudioFilesFound = false;

            foreach (string filePath in Directory.GetFiles(audioDirectory))
            {
                string extention = System.IO.Path.GetExtension(filePath).ToLower();
                if(extention != ".mp3" && extention != ".wav")
                {
                    nonAudioFilesFound = true;
                    try
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                            filePath,
                            Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                            Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to move file '{filePath}' to the Recycle Bin: {ex.Message}.\nPlease keep user/AppData/Local/MeiMusic/playlist folder clean of only audio files to allow proper functionality.");
                    }
                }
            }

            if (nonAudioFilesFound)
            {
                MessageBox.Show("Some Non-mp3/wav files are detected; they have been moved to the trash bin.\nPlease keep user/AppData/Local/MeiMusic/playlist folder clean of only audio files to allow proper functionality.");
            }

            // Step 2: Delete duplicate file
            var mp3Files = new HashSet<string>();
            foreach (string filePath in Directory.GetFiles(audioDirectory))
            {
                string extension = System.IO.Path.GetExtension(filePath).ToLower();
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath);

                if (extension == ".mp3")
                {
                    // Track .mp3 files by base name
                    mp3Files.Add(fileNameWithoutExtension);
                }
                else if (extension == ".wav" && mp3Files.Contains(fileNameWithoutExtension))
                {
                    // If .wav file has a corresponding .mp3 file, delete the .wav version
                    try
                    {
                        Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(
                            filePath,
                            Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                            Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);

                        MessageBox.Show($"Duplicate found for '{fileNameWithoutExtension}'. The .wav version has been deleted.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete duplicate .wav file '{filePath}': {ex.Message}\nPlease don't have duplicate file name in the user/AppData/Local/MeiMusic/playlist folder to allow proper functionality.");
                    }
                }
            }

            // Step 3: Sync UI with Folder - Remove items from the UI if their corresponding files no longer exist
            var filesInFolder = new HashSet<string>(Directory.GetFiles(audioDirectory)
                .Where(file => System.IO.Path.GetExtension(file).ToLower() == ".mp3" || System.IO.Path.GetExtension(file).ToLower() == ".wav")
                .Select(file => System.IO.Path.GetFileNameWithoutExtension(file)));

            for (int i = UploadedSongList.Items.Count - 1; i >= 0; i--)
            {
                string? fileNameInUI = UploadedSongList.Items[i]?.ToString();
                if (fileNameInUI != null && !filesInFolder.Contains(fileNameInUI))
                {
                    UploadedSongList.Items.RemoveAt(i);
                }
            }

            // Step 4: Add missing MP3/WAV files to UI
            foreach (string filePath in Directory.GetFiles(audioDirectory))
            {
                string extension = System.IO.Path.GetExtension(filePath).ToLower();
                if (extension == ".mp3" || extension == ".wav")
                {
                    string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath);
                    bool fileAlreadyInUI = false;
                    foreach (var item in UploadedSongList.Items)
                    {
                        if (item.ToString() == fileNameWithoutExtension)
                        {
                            fileAlreadyInUI = true;
                            break;
                        }
                    }
                    if (!fileAlreadyInUI)
                    {
                        AddFileToUI(filePath);
                    }
                }
            }

        }
        // Event handler for the Refresh Button
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshSongsInUI();
        }
        //----------------------------------------------------------------------------------


        //------------------------- Manage Close Drop Down ---------------------------------
        private void OnMainWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Close the Popup if the click is outside the Popup
            if (PlusPopupMenu.IsOpen && !PlusPopupMenu.IsMouseOver)
            {
                PlusPopupMenu.IsOpen = false;
                PlusButton.IsEnabled = true; // Re-enable the button when dropdown closes
            }
            if (SortPopupMenu.IsOpen && !SortPopupMenu.IsMouseOver)
            {
                SortPopupMenu.IsOpen = false;
                SortButton.IsEnabled = true;
            }
        }
        private void Window_Deactivated(object sender, EventArgs e)
        {
            // Close the dropdown if the application loses focus
            if (PlusPopupMenu.IsOpen)
            {
                PlusPopupMenu.IsOpen = false;
                PlusButton.IsEnabled = true; // Ensure the button is re-enabled
            }
            if (SortPopupMenu.IsOpen)
            {
                SortPopupMenu.IsOpen = false;
                SortButton.IsEnabled = true;
            }
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