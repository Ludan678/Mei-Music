App name: Mei Music

Description: a music app that turns any kind of mp4 or videos imported from the user into sound files. Users are able to create a list of these sound files to use like a music app, where the music list can be played consecutively in the background. 2 ways of importing: 1. From a local photo or local files app in the device. 2. A link tractor where you can insert a link to the video and the software will auto detect the video file and convert it into an audio file and put it in the music list.

Target platform, currently: windows

Usage:
turn a video file from a local file or local photo into an audio file and put it in the user playlist. 
detect videos from links and turn it into audio file and put it in the user playlist
The user can give their own desirable name to the songs in the list
The user can click on any of the songs in the list to play
Once the song finishes, it will auto move on to the next one, once it hit the end, it will loop back to the first song
Allow the deletion of the songs in the list

Roadmap to Building Mei Music
1. Core Features Recap
Convert MP4 and video files into audio files.
Import videos from local files and extract the audio.
Download and convert audio from video URLs.
Create and manage playlists (rename, delete, play, loop).
Background playback support.
Lightweight interface, optimized for Windows users.
Tools & Technologies to Use
Language & Framework:
C# WPF (Windows Presentation Foundation): works only on Windows
Audio & Video Processing:
FFmpeg for video-to-audio conversion.
Data Storage:
SQLite for playlists and metadata storage.
Project Structure
Frontend (UI):
Home screen displaying playlists.
Buttons for importing local files or adding video URLs.
Controls for play, pause, next, previous, and loop.
Backend:
Handle video conversion to audio using FFmpeg.
Store playlist and song metadata.
Manage playback and looping behavior.
How Google Login Works
User Authentication: Redirect the user to Google’s sign-in page.
Authorization: On success, Google returns an authentication token.
User Creation: Use the token to fetch user information (like name or email) and create an account in your app.
Store User Data: Save the user's login state, either in SQLite or local storage, so they don’t need to log in every time.
Additional Features (Future Enhancements)
Drag-and-Drop Support: Allow users to drag video files directly into the playlist.
Volume and Seek Controls: Add more media player controls.
Dark Mode UI: Improve the look and feel with a sleek dark theme.
Windows Tray Icon: Add an option to minimize the app to the system tray.

xaml : define the looks of UI
xaml.cs : define the behaviors of UI



Notes

In WPF, the term "Preview" in an event name, such as PreviewMouseLeftButtonDown, indicates that it is a "preview" (or tunneling) event rather than a standard (or bubbling) event.
Here’s the difference:
Tunneling vs. Bubbling Events
Tunneling Events (Preview Events):
Tunneling events begin at the root of the visual tree and "tunnel" down to the target element where the event occurred.
These events are prefixed with "Preview" (e.g., PreviewMouseLeftButtonDown, PreviewKeyDown).
You can think of tunneling events as having a "top-down" approach. For example, the PreviewMouseLeftButtonDown event will first be raised on the top-level container (like a Window), then on each child container, and finally on the target element itself.
Bubbling Events:
Bubbling events begin at the element that triggered the event and "bubble" up through the parent elements in the visual tree.
These events do not have a "Preview" prefix (e.g., MouseLeftButtonDown, KeyDown).
Bubbling events have a "bottom-up" approach, so the MouseLeftButtonDown event is first raised on the target element and then propagates up through each parent container until it reaches the root.
Why Use Preview (Tunneling) Events?
Intercept Events Earlier: Preview events allow parent containers to handle or cancel an event before it reaches the target element. This can be useful for setting global rules, validations, or preventing certain actions within a container.
Prevent Event Propagation: By handling a preview event on a container, you can prevent it from being passed down to specific child elements. This is helpful when you want to intercept and modify behavior at a higher level in the UI hierarchy.
Example
If you want to intercept a mouse click on a Button but still respond to the event at a container level (such as a Grid or Window), you could use PreviewMouseLeftButtonDown. The event would trigger on the container first, allowing you to modify or cancel it before the Button itself responds.
In short, "Preview" indicates that the event is a tunneling event that allows you to handle it on parent elements before it reaches the target element.

















