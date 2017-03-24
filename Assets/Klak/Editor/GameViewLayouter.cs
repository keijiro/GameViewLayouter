using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace Klak
{
    class GameViewLayouter : EditorWindow
    {
        #region Editor resources

        static GUIContent _textViewCount = new GUIContent("Number of Screens");

        static GUIContent[] _viewLabels = {
            new GUIContent("Screen 2"),
            new GUIContent("Screen 3"), new GUIContent("Screen 4"),
            new GUIContent("Screen 5"), new GUIContent("Screen 6"),
            new GUIContent("Screen 7"), new GUIContent("Screen 8"),
        };

        static GUIContent[] _optionLabels = {
            new GUIContent("Display 1"), new GUIContent("Display 2"),
            new GUIContent("Display 3"), new GUIContent("Display 4"),
            new GUIContent("Display 5"), new GUIContent("Display 6"),
            new GUIContent("Display 7"), new GUIContent("Display 8"),
        };

        static int[] _optionValues = { 0, 1, 2, 3, 4, 5, 6, 7 };

        #endregion

        #region Private variables

        [SerializeField] GameViewLayoutTable _table;

        #endregion

        #region UI methods

        [MenuItem("Window/Game View Layouter")]
        static void OpenWindow()
        {
            EditorWindow.GetWindow<GameViewLayouter>("Layouter").Show();
        }

        void OnGUI()
        {
            var serializedTable = new UnityEditor.SerializedObject(_table);

            EditorGUILayout.BeginVertical();

            // Screen num box
            var viewCount = serializedTable.FindProperty("viewCount");
            EditorGUILayout.PropertyField(viewCount, _textViewCount);

            EditorGUILayout.Space();

            // View-display table
            var viewTable = serializedTable.FindProperty("viewTable");
            for (var i = 0; i < viewCount.intValue; i++)
            {
                EditorGUILayout.IntPopup(
                    viewTable.GetArrayElementAtIndex(i),
                    _optionLabels, _optionValues, _viewLabels[i]
                );
            }

            EditorGUILayout.Space();

            // Function buttons
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Layout")) LayoutViews();
            EditorGUILayout.Space();
            if (GUILayout.Button("Close All")) CloseAllViews();
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            serializedTable.ApplyModifiedProperties();
        }

        #endregion

        #region Private properties and methods

        // Retrieve the hidden GameView type.
        static Type GameViewType {
            get { return System.Type.GetType("UnityEditor.GameView,UnityEditor"); }
        }

        // Change the target display of a game view.
        static void ChangeTargetDisplay(EditorWindow view, int displayIndex)
        {
            var serializedObject = new SerializedObject(view);
            var targetDisplay = serializedObject.FindProperty("m_TargetDisplay");
            targetDisplay.intValue = displayIndex;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        // Close all the game views.
        static void CloseAllViews()
        {
            foreach (EditorWindow view in Resources.FindObjectsOfTypeAll(GameViewType))
                view.Close();
        }

        // Send a game view to a given screen.
        static void SendViewToScreen(EditorWindow view, int screenIndex)
        {
            const int kMenuHeight = 22;

            var res = Screen.currentResolution;
            var origin = new Vector2(res.width * screenIndex, - kMenuHeight);
            var size = new Vector2(res.width, res.height + kMenuHeight);

            view.position = new Rect(origin, size);
            view.minSize = view.maxSize = size;
            view.position = new Rect(origin, size);
        }

        // Instantiate and layout game views based on the setting.
        void LayoutViews()
        {
            CloseAllViews();

            for (var i = 0; i < _table.viewCount; i++)
            {
                var view = (EditorWindow)ScriptableObject.CreateInstance(GameViewType);
                view.Show();
                ChangeTargetDisplay(view, _table.viewTable[i]);
                SendViewToScreen(view, i + 1);
            }
        }

        #endregion
    }
}
