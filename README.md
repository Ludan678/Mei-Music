App name: Mei Music
Made with: WPF, xaml + c#
This is only the source code, not the released sofeware product version. Can be opened with VS code.
Once you run this once in vs code, you can find the released version under Mei Music/bin/release which can be used with saved user data.
----------------------------------------------------------------
Create your Own music SoundTrack of any kind 
----------------------------------------------------------------
Description: a music app that turns any imported audio or videos files into a list of playable music playlists.

Supported Files: video [mp4, mkv] audio [wav, mp3]

More:
Users are able to create a list of these imported sound files to use like a music app, where the music list can be played consecutively in loop in the background. 
[Note: this means that this is not a app where you are searching for songs inside the app]
-

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
Left Panel [currently empty for future implementations]
Traditional PREVIOUS, STOP, NEXT in music playlist. Works with looping.

Playlist:
click on the imported or extracted song to play it.
Options attached to each song
 -rename
 -change volumn of the song [as songs imported may not be the same level volumn]
 -open song in folder [where the playlists are stored]
 -delete song button 





















