using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using com.DartsGames.SlimeShopManage._Scripts._Code.Scripts.NodeBasedEditor;
using SlimesShopManager.Core.SlimeEvolveTree;
using UnityEditor;
using UnityEngine;

public class Connection : ScriptableObject, IGetId
{
    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;
    [XmlIgnore] public Action<Connection> OnClickConnection;
    [XmlIgnore] public Action<Connection> OnClickRemoveConnection;
    private int _id;

    public int Id
    {
        get => _id;
        set => _id = value;
    }

    public EvolveTransition evolveTransition;
    
    private ConnectionCondition selectedCondition;
    
    private float lineWidth;

    public Connection()
    {
    }

    public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint)
    {
        this.inPoint = inPoint;
        this.outPoint = outPoint;
    }
    public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint,
        Action<Connection> OnClickConnection,
        Action<Connection> OnClickRemoveConnection)
    {
        this.inPoint = inPoint;
        this.outPoint = outPoint;
        this.OnClickConnection = OnClickConnection;
        this.OnClickRemoveConnection = OnClickRemoveConnection;

        lineWidth = 3f;
    }

    public void DrawConnection()
    {
        Handles.DrawBezier(
            inPoint.rect.center,
            outPoint.rect.center,
            inPoint.rect.center + Vector2.left * 50f,
            outPoint.rect.center - Vector2.left * 50f,
            Color.white,
            null,
            lineWidth
        );

        if (Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f, Quaternion.identity, 4, 8,
            Handles.RectangleHandleCap))
        {
            OnClickRemoveConnection?.Invoke(this);
        }
    }

    public bool ProcessEvent(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    var distance = HandleUtility.DistancePointBezier(e.mousePosition,
                        inPoint.rect.center,
                        outPoint.rect.center,
                        inPoint.rect.center + Vector2.left * 50f,
                        outPoint.rect.center - Vector2.left * 50f);
                    if (distance < 1f)
                    {
                        OnClickConnection?.Invoke(this);
                        UnityEditor.Selection.activeObject = this;
                        lineWidth = 5f;
                        GUI.changed = true;
                        return true;
                    }
                    if (distance >= 1f)
                    {
                        lineWidth = 3f;
                        GUI.changed = true;
                        return false;
                    }
                }
                break;
        }
        return false;
    }
}