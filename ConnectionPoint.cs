using System;
using System.Xml.Serialization;
using UnityEngine;

public enum ConnectionPointType { In, Out }

public class ConnectionPoint
{
    public string id;
    
    [XmlIgnore] public Rect rect;

    [XmlIgnore] public ConnectionPointType type;

    [XmlIgnore] public Node node;

    [XmlIgnore] public GUIStyle style;

    [XmlIgnore] public Action<ConnectionPoint> OnClickConnectionPoint;

    public ConnectionPoint() { }

    public ConnectionPoint(Node node, string id)
    {
        this.node = node;
        this.id = id;
    }
    public ConnectionPoint(Node node, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> OnClickConnectionPoint, string id = null)
    {
        this.node = node;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);

        this.id = id ?? Guid.NewGuid().ToString();
    }

    public void Draw()
    {
        rect.y = node.Rect.y + (node.Rect.height * 0.5f) - rect.height * 0.5f;

        rect.x = type switch
        {
            ConnectionPointType.In => node.Rect.x - rect.width + 8f,
            ConnectionPointType.Out => node.Rect.x + node.Rect.width - 8f,
            _ => rect.x
        };

        if (GUI.Button(rect, "", style))
        {
            OnClickConnectionPoint?.Invoke(this);
        }
    }
}