namespace MindArrow.Core
{
    public static class GameSession
    {
        public const int FirstLevelNumber = 1;

        public static int SelectedLevelNumber { get; private set; } = FirstLevelNumber;

        public static void SelectLevel(int levelNumber)
        {
            SelectedLevelNumber = levelNumber < FirstLevelNumber
                ? FirstLevelNumber
                : levelNumber;
        }

        public static void ResetToFirstLevel()
        {
            SelectedLevelNumber = FirstLevelNumber;
        }
    }
}
