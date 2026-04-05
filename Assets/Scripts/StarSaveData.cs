using UnityEngine;

public static class StarSaveData
{
    public static bool hasData = false;

    // lưu star đã đạt
    public static bool[] collected;

    public static void Save(bool[] stars)
    {
        collected = (bool[])stars.Clone();
        hasData = true;
    }

    public static void Clear()
    {
        hasData = false;
        collected = null;
    }
}