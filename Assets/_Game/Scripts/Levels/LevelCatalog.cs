using System.Collections.Generic;
using UnityEngine;

namespace MindArrow.Levels
{
    [CreateAssetMenu(
        fileName = "LevelCatalog",
        menuName = "MindArrow/Level Catalog",
        order = 1)]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField]
        private List<LevelData> levels = new();

        public IReadOnlyList<LevelData> Levels => levels;
        public int Count => levels != null ? levels.Count : 0;

        public bool TryGetByNumber(int levelNumber, out LevelData level)
        {
            level = null;

            if (levels == null)
            {
                return false;
            }

            for (int i = 0; i < levels.Count; i++)
            {
                LevelData candidate = levels[i];

                if (candidate == null ||
                    candidate.LevelNumber != levelNumber)
                {
                    continue;
                }

                if (!candidate.IsValid(out string error))
                {
                    Debug.LogWarning(
                        $"Ignoring invalid LevelData '{candidate.name}': {error}",
                        candidate);
                    return false;
                }

                level = candidate;
                return true;
            }

            return false;
        }

        public bool TryGetNext(int currentLevelNumber, out LevelData nextLevel)
        {
            nextLevel = null;

            if (levels == null)
            {
                return false;
            }

            int bestNumber = int.MaxValue;

            for (int i = 0; i < levels.Count; i++)
            {
                LevelData candidate = levels[i];

                if (candidate == null ||
                    candidate.LevelNumber <= currentLevelNumber ||
                    candidate.LevelNumber >= bestNumber)
                {
                    continue;
                }

                if (!candidate.IsValid(out _))
                {
                    continue;
                }

                bestNumber = candidate.LevelNumber;
                nextLevel = candidate;
            }

            return nextLevel != null;
        }

        public LevelData GetFirst()
        {
            if (levels == null || levels.Count == 0)
            {
                return null;
            }

            LevelData first = null;

            for (int i = 0; i < levels.Count; i++)
            {
                LevelData candidate = levels[i];

                if (candidate == null || !candidate.IsValid(out _))
                {
                    continue;
                }

                if (first == null ||
                    candidate.LevelNumber < first.LevelNumber)
                {
                    first = candidate;
                }
            }

            return first;
        }
    }
}
