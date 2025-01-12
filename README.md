App name: Mei Music
Made with: WPF, xaml + c#
----------------------------------------------------------------
Main Utility: Create your Own music SoundTrack of any kind 
----------------------------------------------------------------
[This is only the source code, not the released sofeware product version. Can be opened with VS code. Once you run this once in vs code, you can find the released version under Mei Music/bin/release which can be used with saved user data.]

Description: a music app that turns any imported audio or videos files into a list of playable music playlists.
Supported Files: video [mp4, mkv] audio [wav, mp3]

More:
Users are able to create a list of these imported sound files to use like a music app, where the music list can be played consecutively in loop in the background. 
[Note: this means that this is not a app where you are searching for songs inside the app]

Ways of importing: 
1. Upload from Computer
  - From the local Computer, simply by dropping in local files.
2. Search Through URL [!Does not function 100% for all videos from all sites OR Video that is > 8 minutes!]
  Working Sites: Bilibili, Youtube[part]
  - A link tractor where you can insert a link to the video online, the software will auto detect the video file and convert it into an audio file and put it in the music list.

The extraction of video uses the open source : yt-dlp.
The conversion of video to audio uses the open source : ffmpeg.
The user will be prompt to enter a username for the loaded song.

Usage:
- Left Panel [currently empty for future implementations]
- Traditional PREVIOUS, STOP, NEXT in music playlist. Works with looping.
- Audio level slider
- Song timer slider

Playlist:
click on the imported or extracted song to play it.
Options attached to each song:
 -rename
 -change volumn of the song [as songs imported may not be the same level volumn]
 -open song in folder [where the playlists are stored]
 -delete song button 

Ordering:
1. order base on alphabet
2. order base on modification date

Refresh:
click this button if your loaded song did not show up

Multiple Songs Importation:
1. Open the folder for the playlist by clicking on open folder from any song.
2. Drag multiple audio files into the folder
3. Click the refresh button in the software

For videos tracked from URL:
- They are not automatically deleted. [some may want to preserve the video]
  can be found at C:\Users\...\AppData\Local\Mei Music\temp\video
- The "empty" Downloaded folder ...\temp\download is used for the process during conversion from video to audio.

These folders are automatically created if deleted unintentionally.

















