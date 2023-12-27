using System;
using System.Collections.Generic;
using System.Linq;
using Code.Editor.SlimeMapEditor;
using Code.Scripts.Attributes;
using DartsGames.Extensions;
using SlimesShopManager.Core.SlimeEvolveTree;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace com.DartsGames.SlimeShopManage._Scripts._Code.Editor.SlimeMapEditor
{
    public class NodeBasedEditor : EditorWindow
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as SlimesEvolveData;
            if (asset != null)
            {
                Init(asset);
                return true;
            }

            return false;
        }

        private static void Init(SlimesEvolveData argTree)
        {
            var window = GetWindow<NodeBasedEditor>();
            window._treeData = argTree;
            window.Initialize();
            window.Show();
        }

        private SlimesEvolveData _treeData;

        private List<Node> nodes = new();
        private List<Connection> connections = new();

        #region GUI Syles

        private GUIStyle nodeStyle;
        private GUIStyle selectedNodeStyle;
        private GUIStyle inPointStyle;
        private GUIStyle outPointStyle;

        #endregion

        private ConnectionPoint selectedInPoint;
        private ConnectionPoint selectedOutPoint;

        private Vector2 offset;
        private Vector2 drag;

        private readonly float menuBarHeight = 20f;

        private Rect menuBar;
        private Rect mainSectionRect;
        private Rect conditionsSectionRect;
        private Rect resizerRect;
        private Rect typeSectionRect;

        private float sizeRatio = 0.7f;
        private bool isResizing;

        private ConnectionInspectorEditor _connectionInspectorEditor;
        private SlimeTypeWindowEditor _slimeTypeWindowEditor;

        private void OnEnable()
        {
            _connectionInspectorEditor = new ConnectionInspectorEditor();
            _slimeTypeWindowEditor = CreateInstance<SlimeTypeWindowEditor>();


            foreach (var type in Enum.GetNames(typeof(SlimeType)))
            {
                _slimeTypeWindowEditor.AddSlimeType(type);
            }

            _slimeTypeWindowEditor.Initialize();
        }


        private Node GetNodeById(int argId)
        {
            return nodes.First(x => x.id == argId);
        }

        #region Initialization

        private void InitializeStyle()
        {
            nodeStyle = new GUIStyle
            {
                normal =
                {
                    background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D,
                    textColor = Color.white
                },
                border = new RectOffset(12, 12, 12, 12),
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16
            };

            selectedNodeStyle = new GUIStyle
            {
                normal =
                {
                    background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D,
                    textColor = Color.white
                },
                border = new RectOffset(12, 12, 12, 12),
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 18
            };

            inPointStyle = new GUIStyle
            {
                normal =
                {
                    background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D
                },
                active =
                {
                    background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D
                },
                border = new RectOffset(4, 4, 12, 12)
            };

            outPointStyle = new GUIStyle
            {
                normal =
                {
                    background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D
                },
                active =
                {
                    background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D
                },
                border = new RectOffset(4, 4, 12, 12)
            };
        }
        private void Initialize()
        {
            InitializeStyle();

            foreach (var stage in _treeData.EvolveStages)
            {
                //Stage to node
                var nodeSo = ScriptableObject.CreateInstance<Node>();
                nodeSo.id = stage.Id;
                nodeSo.style = nodeStyle;
                nodeSo.selectedNodeStyle = selectedNodeStyle;
                nodeSo.inPoint = new ConnectionPoint(nodeSo, ConnectionPointType.In, inPointStyle, OnClickInPoint, $"In_{stage.Id}");
                nodeSo.outPoint = new ConnectionPoint(nodeSo, ConnectionPointType.Out, outPointStyle, OnClickOutPoint, $"Out_{stage.Id}");
                nodeSo.OnRemoveNode = OnClickRemoveNode;
                nodeSo.slimeEvolveStage = stage;

                nodes.Add(nodeSo);
            }

            foreach (var transition in _treeData.EvolveTransitions)
            {
                var fromNode = GetNodeById(transition.fromId);
                var toNode = GetNodeById(transition.toId);

                var connection = ScriptableObject.CreateInstance<Connection>();
                connection.evolveTransition = transition;
                connection.OnClickConnection = OnClickConnection;
                connection.OnClickRemoveConnection = OnClickRemoveConnection;
                connection.inPoint = toNode.inPoint;
                connection.outPoint = fromNode.outPoint;

                connections.Add(connection);
            }
        }

        #endregion

        private void OnGUI()
        {
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);

            DrawConnections();
            DrawNodes();
            //DrawMenuBar();
            // DrawResizer();
            // DrawSideBar();
            DrawTypeSection();

            if (!typeSectionRect.Contains(Event.current.mousePosition))
            {
                DrawConnectionLine(Event.current);

                ProcessNodeEvents(Event.current);
                ProcessConnectionEvents(Event.current);
                ProcessResizerEvents(Event.current);
                ProcessEvents(Event.current);
            }

            if (GUI.changed) Repaint();
        }

        private void DrawMenuBar()
        {
            menuBar = new Rect(0, 0, position.width, menuBarHeight);

            GUILayout.BeginArea(menuBar, EditorStyles.toolbar);
            GUILayout.BeginHorizontal();

            GUILayout.Space(5);

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawSideBar()
        {
            conditionsSectionRect = new Rect(position.width * sizeRatio, menuBarHeight,
                position.width * (1 - sizeRatio), position.height - menuBarHeight);
            GUILayout.BeginArea(conditionsSectionRect, GUI.skin.box);
            _connectionInspectorEditor?.OnGUI();
            GUILayout.EndArea();
        }

        private void DrawTypeSection()
        {
            typeSectionRect = new Rect(0, position.height / 2, 300, position.height / 2);
            GUILayout.BeginArea(typeSectionRect, GUI.skin.box);
            _slimeTypeWindowEditor?.OnGUI();
            GUILayout.EndArea();
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            offset += drag * 0.5f;
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset,
                    new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset,
                    new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void DrawNodes()
        {
            if (nodes != null)
            {
                for (var i = 0; i < nodes.Count; i++)
                {
                    nodes[i].Draw();
                }
            }
        }

        private void DrawConnections()
        {
            if (connections != null)
            {
                for (var i = 0; i < connections.Count; i++)
                {
                    connections[i].DrawConnection();
                }
            }
        }

        private void DrawResizer()
        {
            resizerRect = new Rect((position.width * sizeRatio) - 5f, 0, 10f, position.height);

            GUILayout.BeginArea(new Rect(resizerRect.position + (Vector2.up * 5f), new Vector2(position.width, 2)),
                GUIStyle.none);
            GUILayout.EndArea();

            EditorGUIUtility.AddCursorRect(resizerRect, MouseCursor.ResizeHorizontal);
        }

        private void ProcessEvents(Event e)
        {
            drag = Vector2.zero;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        ClearConnectionSelection();
                    }

                    if (e.button == 1)
                    {
                        ProcessContextMenu(e.mousePosition);
                    }

                    break;

                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        // mainSectionRect = new Rect(0, menuBarHeight,
                        //     position.width * sizeRatio, position.height - menuBarHeight);
                        // if (mainSectionRect.Contains(e.mousePosition))
                        // {
                            OnDrag(e.delta);
                        //}
                    }

                    break;
            }
        }

        private void ProcessNodeEvents(Event e)
        {
            if (nodes != null)
            {
                for (var i = nodes.Count - 1; i >= 0; i--)
                {
                    var guiChanged = nodes[i].ProcessEvents(e);

                    if (guiChanged)
                    {
                        GUI.changed = true;
                    }
                }
            }
        }

        private void ProcessConnectionEvents(Event e)
        {
            if (connections is not null)
            {
                for (var i = connections.Count - 1; i >= 0; i--)
                {
                    var guiChanged = connections[i].ProcessEvent(e);

                    if (guiChanged)
                    {
                        GUI.changed = true;
                    }
                }

                // if (connections.All(connection => !connection.ProcessEvent(e)))
                // {
                //     _connectionInspectorEditor.IsSelected = false;
                // }
            }
        }

        private void ProcessResizerEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0 && resizerRect.Contains(e.mousePosition))
                    {
                        isResizing = true;
                    }

                    break;

                case EventType.MouseUp:
                    isResizing = false;
                    break;
            }

            Resize(e);
        }

        private void Resize(Event e)
        {
            if (!isResizing) return;

            sizeRatio = e.mousePosition.x / position.width;
            Repaint();
        }

        private void DrawConnectionLine(Event e)
        {
            if (selectedInPoint != null && selectedOutPoint == null)
            {
                Handles.DrawBezier(
                    selectedInPoint.rect.center,
                    e.mousePosition,
                    selectedInPoint.rect.center + Vector2.left * 50f,
                    e.mousePosition - Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );

                GUI.changed = true;
            }

            if (selectedOutPoint != null && selectedInPoint == null)
            {
                Handles.DrawBezier(
                    selectedOutPoint.rect.center,
                    e.mousePosition,
                    selectedOutPoint.rect.center - Vector2.left * 50f,
                    e.mousePosition + Vector2.left * 50f,
                    Color.white,
                    null,
                    2f
                );

                GUI.changed = true;
            }
        }

        private void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Add node"), false, () => OnClickAddNode(mousePosition));
            genericMenu.ShowAsContext();
        }

        private void OnDrag(Vector2 delta)
        {
            drag = delta;

            if (nodes != null)
            {
                for (var i = 0; i < nodes.Count; i++)
                {
                    nodes[i].Drag(delta);
                }
            }

            GUI.changed = true;
        }

        private void OnClickAddNode(Vector2 mousePosition)
        {
            var stage = new SlimeEvolveStage
            {
                rect = new Rect(mousePosition, new Vector2(200, 70))
            };
            _treeData.AddStage(stage);

            nodes ??= new List<Node>();

            nodes.Add(new Node(
                nodeStyle,
                selectedNodeStyle,
                inPointStyle,
                outPointStyle,
                OnClickInPoint,
                OnClickOutPoint,
                OnClickRemoveNode,
                stage));
        }

        private void OnClickInPoint(ConnectionPoint inPoint)
        {
            selectedInPoint = inPoint;

            if (selectedOutPoint != null)
            {
                if (selectedOutPoint.node != selectedInPoint.node)
                {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else
                {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickOutPoint(ConnectionPoint outPoint)
        {
            selectedOutPoint = outPoint;

            if (selectedInPoint != null)
            {
                if (selectedOutPoint.node != selectedInPoint.node)
                {
                    CreateConnection();
                    ClearConnectionSelection();
                }
                else
                {
                    ClearConnectionSelection();
                }
            }
        }

        private void OnClickRemoveNode(Node node)
        {
            if (connections != null)
            {
                List<Connection> connectionsToRemove = new List<Connection>();

                for (int i = 0; i < connections.Count; i++)
                {
                    if (connections[i].inPoint == node.inPoint || connections[i].outPoint == node.outPoint)
                    {
                        connectionsToRemove.Add(connections[i]);
                    }
                }

                for (int i = 0; i < connectionsToRemove.Count; i++)
                {
                    connections.Remove(connectionsToRemove[i]);
                }

                connectionsToRemove = null;
            }

            nodes.Remove(node);
        }

        private void OnClickConnection(Connection connection)
        {
            _connectionInspectorEditor.SelectedConnection = connection;
            //_connectionInspectorEditor.IsSelected = true;
        }

        private void OnClickRemoveConnection(Connection connection)
        {
            connections.Remove(connection);
        }

        private void CreateConnection()
        {
            if (connections == null)
            {
                connections = new List<Connection>();
            }

            var connection = new Connection(selectedInPoint, selectedOutPoint, OnClickConnection,
                OnClickRemoveConnection);
            connection.Id = connections.GetUniqueId();
            connections.Add(connection);
        }

        private void ClearConnectionSelection()
        {
            selectedInPoint = null;
            selectedOutPoint = null;
        }
    }
}