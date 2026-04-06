using UnityEngine;

public static class SaveDataManager
{
    public static bool hasData = false;

    // ⭐ STAR
    public static bool[] collectedStars;

    // 🏆 SCORE
    public static int highestScore = 0;

    // ===== SAVE STAR =====
    public static void SaveStars(bool[] newStars)
    {
        if (!hasData || collectedStars == null)
        {
            collectedStars = (bool[])newStars.Clone();
        }
        else
        {
            // 🔥 MERGE STAR (OR logic)
            for (int i = 0; i < collectedStars.Length; i++)
            {
                collectedStars[i] = collectedStars[i] || newStars[i];
            }
        }

        hasData = true;
    }

    // ===== SAVE SCORE =====
    public static void SaveScore(int score)
    {
        if (score > highestScore)
        {
            highestScore = score;
        }
    }

    public static void Clear()
    {
        hasData = false;
        collectedStars = null;
        highestScore = 0;
    }
}