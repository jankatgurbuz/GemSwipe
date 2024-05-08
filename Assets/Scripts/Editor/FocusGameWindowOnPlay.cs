using UnityEditor;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public class FocusGameWindowOnPlay
    {
        static FocusGameWindowOnPlay()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                FocusGameWindow();
            }
        }

        private static void FocusGameWindow()
        {
            EditorWindow gameWindow = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            if (gameWindow != null)
            {
                gameWindow.Focus();
            }
        }
    }
}