using UnityEngine;

namespace MindArrow.Save
{
    public static class SaveService
    {
        private const string UnlockedLevelKey = "mindarrow.unlocked_level";

        public static int GetUnlockedLevel()
        {
            return Mathf.Max(
                1,
                PlayerPrefs.GetInt(UnlockedLevelKey, 1));
        }

        public static bool IsLevelUnlocked(int levelNumber)
        {
            return levelNumber <= GetUnlockedLevel();
        }

        public static void UnlockUpTo(int levelNumber)
        {
            int sanitized = Mathf.Max(1, levelNumber);
            int current = GetUnlockedLevel();

            if (sanitized <= current)
            {
                return;
            }

            PlayerPrefs.SetInt(UnlockedLevelKey, sanitized);
            PlayerPrefs.Save();
        }

        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey(UnlockedLevelKey);
            PlayerPrefs.Save();
        }
    }
}
