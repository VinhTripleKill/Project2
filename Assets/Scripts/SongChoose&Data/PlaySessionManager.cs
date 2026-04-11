using UnityEngine;

public static class PlaySessionManager
{
    public static SongData currentSong;

    public static void SetSong(SongData song)
    {
        currentSong = song;
    }

    public static void Clear()
    {
        currentSong = null;
    }
}