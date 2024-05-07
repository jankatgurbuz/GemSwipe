using System;
using System.Linq;
using System.Reflection;
using BoardItems;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Editor
{
    public class LevelGenerator : EditorWindow
    {
        private const int RowLength = 8;
        private const int ColumnLength = 8;

        private GUISkin _skin;
        private IBoardItem[,] _boardItem;
        private bool[] _toggles;

        [MenuItem("GemSwipe/Level Generator")]
        public static void ShowWindow()
        {
            var windowSize = new Vector2(500, 500);
            var window = GetWindow<LevelGenerator>("LevelGenerator");
            window.minSize = windowSize;
            window.maxSize = windowSize;
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            const string skinPath = "Assets/Scripts/Editor/EditorSkin.guiskin";
            _skin = AssetDatabase.LoadAssetAtPath<GUISkin>(skinPath);
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Level Generate"))
                Generate();

            if (_boardItem == null)
                return;

            ShowToggle();
            ShowItems();

            if (GUILayout.Button("Save"))
                Save();
        }

        private void Save()
        {
            var levelData = CreateInstance<SOLevelData>();
            
            levelData.RowLength = RowLength;
            levelData.ColumnLength = ColumnLength;
            levelData.BoardItem = new IBoardItem[RowLength * ColumnLength];
            levelData.ColumnGenerationFlags = _toggles.ToList();

            for (int i = 0; i < RowLength; i++)
            {
                for (int j = 0; j < ColumnLength; j++)
                {
                    levelData.BoardItem[i * ColumnLength + j] = _boardItem[i, j];
                }
            }
            
            var path = EditorUtility.SaveFilePanelInProject(
                "Save Level Data",
                "Level1",
                "asset",
                "Please enter a file name to save the level data.",
                "Assets/Levels/"
            );

            if (path.Length != 0)
            {
                AssetDatabase.CreateAsset(levelData, path);
                AssetDatabase.SaveAssets();
                
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = levelData;
            }
        }

        private void ShowToggle()
        {
            EditorGUILayout.BeginHorizontal();
            for (int column = 0; column < RowLength; column++)
            {
                _toggles[column] = EditorGUILayout.Toggle(_toggles[column], _skin.GetStyle("toggle"));
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ShowItems()
        {
            for (int row = 0; row < RowLength; row++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int column = 0; column < ColumnLength; column++)
                {
                    switch (((Gem)_boardItem[row, column]).Color)
                    {
                        case ItemColors.Red:
                            GUILayout.Label("", _skin.GetStyle("red"));
                            break;
                        case ItemColors.Yellow:
                            GUILayout.Label("", _skin.GetStyle("yellow"));
                            break;
                        case ItemColors.Green:
                            GUILayout.Label("", _skin.GetStyle("green"));
                            break;
                        case ItemColors.Blue:
                            GUILayout.Label("", _skin.GetStyle("blue"));
                            break;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void Generate()
        {
            _boardItem = new IBoardItem[RowLength, ColumnLength];
            for (int i = 0; i < RowLength; i++)
            {
                for (int j = 0; j < ColumnLength; j++)
                {
                    _boardItem[i, j] =new Gem(i,j,GetRandomColor());
                }
            }
            
            _toggles = new bool [ColumnLength];

            for (int i = 0; i < ColumnLength; i++)
            {
                _toggles[i] = true;
            }
        }

        private ItemColors GetRandomColor()
        {
            var colors = Enum.GetValues(typeof(ItemColors))
                .Cast<ItemColors>()
                .Where(color => color != ItemColors.Empty)
                .ToArray();
            var randomIndex = Random.Range(0, colors.Length);
            return colors[randomIndex];
        }
    }
}