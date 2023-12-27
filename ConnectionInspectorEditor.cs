using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Code.Editor.SlimeMapEditor
{
    public class ConnectionInspectorEditor
    {
        private ReorderableList list;
        //private Rect listRect;

        private Connection selectedConnection;
        private SerializedObject selectedSO;

        private bool isSelected = true;
        private Vector2 scrollPosition;

        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                Initialize();
            }
        }

        public Connection SelectedConnection
        {
            get => selectedConnection;
            set
            {
                selectedConnection = value;
                Initialize();
            }
        }

        private void Initialize()
        {
            
            selectedSO = new SerializedObject(selectedConnection);
            list = new ReorderableList(selectedSO, selectedSO.FindProperty("evolveTransition").FindPropertyRelative("conditions"), true, true, true, true)
            {
                drawHeaderCallback = (rect) => EditorGUI.LabelField(rect,
                    $"{SelectedConnection.outPoint.node.SlimeType} - {SelectedConnection.inPoint.node.SlimeType} Conditions")
            };

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                rect.y += 2f;
                var condition = list.serializedProperty.GetArrayElementAtIndex(index);

                var elementWidth = rect.width * 0.25f;
                var minValueWidth = rect.width * 0.15f;
                var maxValueWidth = rect.width * 0.15f;

                EditorGUI.PropertyField(
                    new Rect(rect.x + 20, rect.y, elementWidth - 20, EditorGUIUtility.singleLineHeight),
                    condition.FindPropertyRelative("element"),
                    GUIContent.none
                );

                var xOffset = rect.x + elementWidth + 10f;

                EditorGUI.LabelField(new Rect(xOffset, rect.y, minValueWidth, EditorGUIUtility.singleLineHeight),
                    "MinValue");
                xOffset += minValueWidth + 10f;

                EditorGUI.PropertyField(
                    new Rect(xOffset, rect.y, maxValueWidth, EditorGUIUtility.singleLineHeight),
                    condition.FindPropertyRelative("min"),
                    GUIContent.none
                );

                xOffset += maxValueWidth + 10f;

                EditorGUI.LabelField(new Rect(xOffset, rect.y, maxValueWidth, EditorGUIUtility.singleLineHeight),
                    "MaxValue");

                EditorGUI.PropertyField(
                    new Rect(xOffset + maxValueWidth + 10f, rect.y, maxValueWidth, EditorGUIUtility.singleLineHeight),
                    condition.FindPropertyRelative("max"),
                    GUIContent.none
                );
            };
            
        }

        public void OnGUI()
        {
            if (list is null && SelectedConnection is null)
            {
                EditorGUILayout.HelpBox("It is not selected connection.", MessageType.Info);
                return;
            }
            scrollPosition = GUILayout.BeginScrollView(scrollPosition); 

            list.DoLayoutList();
            selectedSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(selectedConnection);
            GUILayout.EndScrollView();
        }
    }
}