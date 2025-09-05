# Colibri v3.0.3 - master branch

![](/Images/logo.png)

My fork of Artem Shuba's Colibri UWP App.

So, this is the UWP version for Windows 10/Mobile (min. os. build: 10240).

## About
My "Project Calibri" is synthez of 2 Artem Shuba's projects: Colibri & Meridian. 
So, it planned as messenger + music player which allows you to listen to music from popular Russian social network [vk.com](https://vk.com). 

## Screenshots
![](/Images/sshot01.png)
![](/Images/sshot02.png)
![](/Images/sshot03.png)
![](/Images/sshot04.png)

## My 2 cents
- VK Api version increased, VK login is ok at now
- Initial state of Messenger+Music Player conjunction  

## Big problem(s)
- New Auth/Login "mechanics" (Android masking) damaged notifications :(
- Astoria 10240 - successfully compiled but app don t want to start…

## What's new (thanks to ChatGPT AI)

- Auto-advance to next track

- Switched to MediaPlayer and hooked MediaPlayer.MediaEnded → calls 

- Next()  to advance in the queue. SetQueue(...)
 
- Next(), Previous()  already in place; now auto-advance uses them.

-Highlight now playing in the list

- Named the lists: MyListView, PopularListView.

- Subscribes to AudioService.PlayStateChanged.

- HighlightNowPlayingInActiveList()  selects and scrolls to the currently playing 
VkAudio  in the active tab.

- Unsubscribes on Unloaded.

- Migrate AudioService to MediaPlayer (remove obsolete warnings)

- Removed BackgroundMediaPlayer usage.

- Added _player: MediaPlayer, uses MediaSource.CreateFromUri.

- Uses PlaybackSession.PlaybackStateChanged for play/pause state and position updates.

- Reworked IsPlaying, Position, Play/Pause/Stop/Seek to MediaPlayer equivalents.


## How to test

Start the app → Music.

Tap a track in any tab.

Mini-player starts playing with album art.

The tapped track is selected in the list.

Let the track finish or tap Next/Previous.

The next item in the queue starts playing automatically.

The selection moves to the playing item and is scrolled into view.

Pull down to refresh lists, then play again; auto-advance and highlighting continue to work.
Notes

If a track has no album thumb, the mini-player artwork remains empty (expected).

Auto-advance loops within the current list (wrap-around).


## TODO / The fields to improve 
- All code exploring/refactoring (UI, controls, localization...)
- Additional tests 
- Try to migrate from vk.com to vk.ru 
- Video section
- Profile section
- Refactor Settings (simplify it!)

## References
- https://github.com/artemshuba/ Great C# Developer
- https://github.com/artemshuba/Colibri/ The original Colibri project (UWP)
- https://github.com/artemshuba/meridian-uwp  The original Meridian project (UWP)
- https://github.com/artemshuba/meridian The original Meridian project (Desktop)

## .. 
As is. No support. RnD only. DIY!

## .
 [m][e] Sep, 6 2025






