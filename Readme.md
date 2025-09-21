# Colibri v3.0.3 - dev branch

![](/Images/logo.png)

My fork of Artem Shuba's Colibri UWP App.

So, this is the UWP version for Windows 10/Mobile (min. os. build: 10240).

Attention: the code in the _dev_ branch may not always be successfully compiled. This branch is for the process of developing, refining, disassembling, and assembling this and that. If there is some stability, then it is in the _master_ branch (but at the moment there is something that risks shutting down on October 1, 2025...)

## About
My "Project Calibri" is synthez of 2 Artem Shuba's projects: Colibri & Meridian. 
So, it planned as messenger + music player which allows you to listen to music from popular Russian social network [vk.com](https://vk.com). 

## Screenshot
![](/Images/screenshot.png)


## My 2 cents
- VK Api version increased, VK login is ok at now
- Initial state of Messenger+Music Player conjunction  
- Migration (from vk.com to vk.ru) complete(d). Oct, 1 2025 ready! :) 


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
- Add some AI magic to some "Popular music fusion" .... ;)

## References
- https://github.com/artemshuba/ Great C# Developer
- https://github.com/artemshuba/Colibri/ The original Colibri project (UWP)
- https://github.com/artemshuba/meridian-uwp  The original Meridian project (UWP)
- https://github.com/artemshuba/meridian The original Meridian project (Desktop)

## .. 
As is. No support. RnD only. DIY!

## .
 [m][e] Sep, 21 2025


![](/Images/footer.png)



