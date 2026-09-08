#if UNITY_EDITOR
using MindArrow.Board;
using MindArrow.Levels;
using UnityEditor;
using UnityEngine;

namespace MindArrow.EditorTools
{
    public static class MindArrowLevelMenu
    {
        private const string LevelsFolder =
            "Assets/_Game/ScriptableObjects/Levels";

        private const string LevelPath =
            LevelsFolder + "/Level_001.asset";

        private const string CatalogPath =
            LevelsFolder + "/LevelCatalog.asset";

        [MenuItem("MindArrow/Create Default Level 001")]
        public static void CreateDefaultLevel()
        {
            EnsureLevelsFolder();

            LevelData existing =
                AssetDatabase.LoadAssetAtPath<LevelData>(LevelPath);

            if (existing != null)
            {
                EditorGUIUtility.PingObject(existing);
                Debug.Log(
                    "Level_001 already exists. Existing asset was NOT overwritten.",
                    existing);
                EnsureCatalogContains(existing);
                return;
            }

            LevelData level = ScriptableObject.CreateInstance<LevelData>();

            SerializedObject so = new(level);
            so.FindProperty("levelNumber").intValue = 1;
            so.FindProperty("displayName").stringValue = "Level 1";
            so.FindProperty("columns").intValue = 10;
            so.FindProperty("rows").intValue = 16;
            so.FindProperty("timeLimitSeconds").intValue = 180;

            SerializedProperty arrows = so.FindProperty("arrows");
            arrows.arraySize = 3;

            WriteArrow(
                arrows.GetArrayElementAtIndex(0),
                1,
                new Color(0.15f, 0.80f, 0.95f, 1f),
                new[]
                {
                    new GridPosition(1, 2),
                    new GridPosition(1, 5),
                    new GridPosition(2, 5),
                    new GridPosition(2, 6)
                });

            WriteArrow(
                arrows.GetArrayElementAtIndex(1),
                2,
                new Color(1f, 0.35f, 0.65f, 1f),
                new[]
                {
                    new GridPosition(2, 8),
                    new GridPosition(2, 9),
                    new GridPosition(6, 9)
                });

            WriteArrow(
                arrows.GetArrayElementAtIndex(2),
                3,
                new Color(0.35f, 0.45f, 1f, 1f),
                new[]
                {
                    new GridPosition(9, 13),
                    new GridPosition(8, 13),
                    new GridPosition(8, 9)
                });

            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(level, LevelPath);
            AssetDatabase.SaveAssets();

            EnsureCatalogContains(level);

            if (level.IsValid(out string error))
            {
                Debug.Log("Created and validated " + LevelPath, level);
            }
            else
            {
                Debug.LogWarning(
                    "Created Level_001, but validation failed: " + error,
                    level);
            }

            EditorGUIUtility.PingObject(level);
        }

        [MenuItem("MindArrow/Open Level Catalog")]
        public static void OpenLevelCatalog()
        {
            EnsureLevelsFolder();

            LevelCatalog catalog =
                AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);

            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
                AssetDatabase.SaveAssets();
            }

            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);
        }

        private static void EnsureCatalogContains(LevelData level)
        {
            LevelCatalog catalog =
                AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);

            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<LevelCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            SerializedObject catalogSo = new(catalog);
            SerializedProperty levels = catalogSo.FindProperty("levels");

            bool exists = false;

            for (int i = 0; i < levels.arraySize; i++)
            {
                if (levels.GetArrayElementAtIndex(i).objectReferenceValue == level)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                int index = levels.arraySize;
                levels.InsertArrayElementAtIndex(index);
                levels.GetArrayElementAtIndex(index).objectReferenceValue = level;
                catalogSo.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(catalog);
            }

            AssetDatabase.SaveAssets();
        }

        private static void EnsureLevelsFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Game/ScriptableObjects"))
            {
                AssetDatabase.CreateFolder(
                    "Assets/_Game",
                    "ScriptableObjects");
            }

            if (!AssetDatabase.IsValidFolder(LevelsFolder))
            {
                AssetDatabase.CreateFolder(
                    "Assets/_Game/ScriptableObjects",
                    "Levels");
            }
        }

        private static void WriteArrow(
            SerializedProperty arrow,
            int id,
            Color color,
            GridPosition[] path)
        {
            arrow.FindPropertyRelative("id").intValue = id;
            arrow.FindPropertyRelative("color").colorValue = color;

            SerializedProperty pathProp = arrow.FindPropertyRelative("path");
            pathProp.arraySize = path.Length;

            for (int i = 0; i < path.Length; i++)
            {
                SerializedProperty point = pathProp.GetArrayElementAtIndex(i);
                point.FindPropertyRelative("x").intValue = path[i].X;
                point.FindPropertyRelative("y").intValue = path[i].Y;
            }
        }
    }
}
#endif
