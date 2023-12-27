using System;
using Code.Scripts.Attributes;
using SlimesShopManager.Core.SlimeEvolveTree;
using UnityEditor;
using UnityEngine;

public class Node : ScriptableObject
{
    public int id;

    [HideInInspector] public bool isDragged;
    [HideInInspector] public bool isSelected;

    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;

    [HideInInspector] public GUIStyle style;
    [HideInInspector] public GUIStyle defaultNodeStyle;
    [HideInInspector] public GUIStyle selectedNodeStyle;

    public Action<Node> OnRemoveNode;

    public SlimeEvolveStage slimeEvolveStage;

    public Rect Rect => slimeEvolveStage.rect;
    public SlimeType SlimeType => slimeEvolveStage.slimeType;

    public Node(GUIStyle nodeStyle, GUIStyle selectedStyle,
        GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint,
        Action<ConnectionPoint> OnClickOutPoint, Action<Node> OnClickRemoveNode,
        SlimeEvolveStage argStage)
    {
        id = argStage.Id;
        style = nodeStyle;
        inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint, $"In_{argStage.Id}");
        outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint,
            $"Out_{argStage.Id}");
        defaultNodeStyle = nodeStyle;
        selectedNodeStyle = selectedStyle;
        OnRemoveNode = OnClickRemoveNode;
        slimeEvolveStage = argStage;
    }


    public void Drag(Vector2 delta)
    {
        slimeEvolveStage.rect.position += delta;
    }

    public void Draw()
    {
        inPoint.Draw();
        outPoint.Draw();

        GUILayout.BeginArea(slimeEvolveStage.rect, EditorStyles.helpBox);

        GUILayout.Label(slimeEvolveStage.slimeType.ToString());
        
        //todo
        // if (slimeEvolveStage.prefab != null)
        // {
        //     GUILayout.Box(AssetPreview.GetAssetPreview(slimeEvolveStage.prefab), GUILayout.Height(70), GUILayout.Width(70));
        //     var rect = GUILayoutUtility.GetLastRect();
        //     rect.height += 30;    
        // }

        #region ToRemove

        // var selectedTypeIndex = (int)slimeEvolveStage.slimeType;
        // selectedTypeIndex = EditorGUILayout.Popup(selectedTypeIndex, Enum.GetNames(typeof(SlimeType)));
        // slimeEvolveStage.slimeType = (SlimeType)selectedTypeIndex;
        //
        // slimeEvolveStage.prefab = (SlimeVisual)EditorGUILayout.ObjectField("Select prefab:", slimeEvolveStage.prefab, typeof(SlimeVisual), true);
        //
        // GUILayout.BeginHorizontal();
        //
        // GUILayout.Label(" Rarity Level:", GUILayout.Width(80));
        //
        // var selectedRarityIndex = (int)slimeEvolveStage.rarityLevel;
        // selectedRarityIndex = EditorGUILayout.Popup(selectedRarityIndex, Enum.GetNames(typeof(RarityLevel)));
        // slimeEvolveStage.rarityLevel = (RarityLevel)selectedRarityIndex;
        //
        // GUILayout.EndHorizontal();

        #endregion

        GUILayout.EndArea();
    }

    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (slimeEvolveStage.rect.Contains(e.mousePosition))
                    {
                        isDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                        style = selectedNodeStyle;
                        UnityEditor.Selection.activeObject = this;
                    }
                    else
                    {
                        GUI.changed = true;
                        isSelected = false;
                        style = defaultNodeStyle;
                    }
                }

                if (e.button == 1 && isSelected && slimeEvolveStage.rect.Contains(e.mousePosition))
                {
                    ProcessContextMenu();
                    e.Use();
                }

                break;

            case EventType.MouseUp:
                isDragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && isDragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }

                break;
        }

        return false;
    }

    private void ProcessContextMenu()
    {
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
        genericMenu.ShowAsContext();
    }

    private void OnClickRemoveNode()
    {
        OnRemoveNode?.Invoke(this);
    }
}