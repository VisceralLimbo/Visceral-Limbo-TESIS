using UnityEngine;

public static class TutorialManager
{
    private const string SeenKey = "TutorialSeen";
    private const string CompletedKey = "TutorialCompleted";

    public static bool HasSeenTutorial()
    {
        return PlayerPrefs.GetInt(SeenKey, 0) == 1;
    }

    public static void MarkTutorialAsSeen()
    {
        PlayerPrefs.SetInt(SeenKey, 1);
        PlayerPrefs.Save();
    }

    public static bool HasCompletedTutorial()
    {
        return PlayerPrefs.GetInt(CompletedKey, 0) == 1;
    }

    public static void CompleteTutorial()
    {
        PlayerPrefs.SetInt(CompletedKey, 1);
        PlayerPrefs.Save();
    }
}
