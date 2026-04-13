using System.Collections.Generic;

[System.Serializable]
public class SongSaveData
{
    public string songName;
    public int highScore;
    public int stars;
    public int rankIndex;
}

[System.Serializable]
public class SaveFile
{
    public List<SongSaveData> songs = new List<SongSaveData>();
}