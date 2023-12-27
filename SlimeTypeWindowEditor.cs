using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace com.DartsGames.SlimeShopManage._Scripts._Code.Editor.SlimeMapEditor
{
    public class SlimeTypeWindowEditor : ScriptableObject
    {
        private ReorderableList list;
        private SerializedObject selectedSO;
        private Vector2 scrollPosition;

        [SerializeField]
        private List<string> slimeTypes = new ();

        public void AddSlimeType(string slimeType)
        {
            slimeTypes.Add(slimeType);
        }

        public void Initialize()
        {
            selectedSO = new SerializedObject(this);

            list = new ReorderableList(selectedSO, selectedSO.FindProperty("slimeTypes"), true, true, true, true)
            {
                drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Slime Types"),
            };
            

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                rect.y += 2f;
                rect.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(
                    rect, list.serializedProperty.GetArrayElementAtIndex(index)
                );
            };
        }
        private void ReplaceSlimeTypes()
        {
            var path = "Assets/com.DartsGames.SlimeShopManage/_Scripts/_Code/Scripts/Attributes/SlimeType.cs";
            var content = "namespace Code.Scripts.Attributes{public enum SlimeType{" + GetItemsAsContent() + "}}";
            
            UpdateFile(path, content);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        private string GetItemsAsContent()
        {
            var content = "";
            var listProperty = selectedSO.FindProperty("slimeTypes");

            for (var i = 0; i < listProperty.arraySize; i++)
            {
                var element = listProperty.GetArrayElementAtIndex(i);
                content += element.stringValue + ",";
            }

            return content;
        }

        private void UpdateFile(string filePath, string newContent)
        {
            if (File.Exists(filePath))
            {
                using var writer = new StreamWriter(filePath);
                writer.Write(newContent);
                writer.Close();
            }
            else
            {
                Debug.Log("File not found: " + filePath);
            }
        }
        public void OnGUI()
        {
            if (list is null)
            {
                EditorGUILayout.HelpBox("Null reference", MessageType.Info);
                return;
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            
            list.DoLayoutList();
            selectedSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(this);
            
            GUILayout.EndScrollView();
            
            if (GUILayout.Button("Save"))
            {
                ReplaceSlimeTypes();
            }

        }

    }
}