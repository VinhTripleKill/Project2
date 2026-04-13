using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveDataManager
{
    private static SaveFile saveData = new SaveFile();
#if UNITY_EDITOR
    private static string path => Application.dataPath + "/save.json";
#else
    private static string path => Application.persistentDataPath + "/save.json";
#endif
    // ===== LOAD =====
    public static void Load()
    {
        if (!File.Exists(path))
        {
            saveData = new SaveFile();
            return;
        }

        string json = File.ReadAllText(path);
        saveData = JsonUtility.FromJson<SaveFile>(json);

        if (saveData == null)
            saveData = new SaveFile();
    }

    // ===== SAVE FILE =====
    public static void SaveToFile()
    {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(path, json);

        Debug.Log("💾 Saved to: " + path);
    }

    // ===== GET SONG DATA =====
    static SongSaveData GetSong(string songName)
    {
        return saveData.songs.Find(s => s.songName == songName);
    }

    static SongSaveData GetOrCreateSong(string songName)
    {
        var song = GetSong(songName);

        if (song == null)
        {
            song = new SongSaveData
            {
                songName = songName,
                highScore = 0,
                stars = 0,
                rankIndex = -1
            };

            saveData.songs.Add(song);
        }

        return song;
    }
    public static void DeleteAllData()
    {
        // 🔥 reset RAM
        saveData = new SaveFile();

        // 🔥 xóa file
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("🗑️ Deleted save file at: " + path);
        }
        else
        {
            Debug.Log("⚠️ No save file to delete");
        }
    }
    // ===== SAVE =====
    public static void SaveScore(string songName, int score)
    {
        var song = GetOrCreateSong(songName);

        if (score > song.highScore)
        {
            song.highScore = score;
            SaveToFile();
        }
    }

    public static void SaveStars(string songName, int stars)
    {
        var song = GetOrCreateSong(songName);

        if (stars > song.stars)
        {
            song.stars = stars;
            SaveToFile();
        }
    }

    public static void SaveRank(string songName, int rankIndex)
    {
        var song = GetOrCreateSong(songName);

        if (song.rankIndex == -1 || rankIndex < song.rankIndex)
        {
            song.rankIndex = rankIndex;
            SaveToFile();
        }
    }

    // ===== LOAD VALUE =====
    public static int GetHighScore(string songName)
    {
        var song = GetSong(songName);
        return song != null ? song.highScore : 0;
    }

    public static int GetStars(string songName)
    {
        var song = GetSong(songName);
        return song != null ? song.stars : 0;
    }

    public static int GetRank(string songName)
    {
        var song = GetSong(songName);
        return song != null ? song.rankIndex : -1;
    }
}