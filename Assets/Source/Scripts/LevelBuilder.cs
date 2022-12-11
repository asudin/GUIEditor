using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LevelBuilder : EditorWindow
{
    private const string _path = "Assets/Source/Prefabs/Houses";

    private Vector2 _scrollPosition;
    private int _selectedElement;
    private List<GameObject> _catalog = new List<GameObject>();
    private bool _building;
    private LayerMask _buildingLayer;

    private GameObject _createdObject;
    private GameObject _parent;

    [MenuItem("Level/Builder")]
    private static void ShowWindow()
    {
        GetWindow(typeof(LevelBuilder));
    }

    private void OnFocus()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
        RefreshCatalog();
    }

    private void OnGUI()
    {
        if (_createdObject != null)
        {
            DestroyImmediate(_createdObject);
        }

        _parent = (GameObject)EditorGUILayout.ObjectField("Parent", _parent, typeof(GameObject), true);
        _buildingLayer = EditorGUILayout.LayerField(_buildingLayer);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        _building = GUILayout.Toggle(_building, "Start building", "Button", GUILayout.Height(60));
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.BeginVertical(GUI.skin.window);
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        DrawCatalog(GetCatalogIcons());
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (_building)
        {
            if (Raycast(out Vector3 contactPoint))
            {
                DrawPointer(contactPoint);
                RotateObject(contactPoint, Color.red);

                if (CheckInput())
                {
                    CreateObject(contactPoint);
                }

                sceneView.Repaint();
            }
        }
    }

    private bool Raycast(out Vector3 contactPoint)
    {
        Ray guiRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        contactPoint = Vector3.zero;

        if (Physics.Raycast(guiRay, out RaycastHit raycastHit, float.PositiveInfinity,
            LayerMask.GetMask(LayerMask.LayerToName(_parent.layer))))
        {
            contactPoint = raycastHit.point;
            return true;
        }

        return false;
    }

    private void DrawPointer(Vector3 position)
    {
        if (_createdObject == null)
            _createdObject = Instantiate(_catalog[_selectedElement]);

        ObjectParentPlacement(position);
    }

    private void DrawHandleBox(Color color, Vector3 position)
    {
        Handles.color = color;
        Bounds bounds = _createdObject.GetComponent<MeshRenderer>().bounds;
        Handles.DrawWireCube(bounds.center, bounds.size);
    }

    private void CreateObject(Vector3 position)
    {
        if (_selectedElement < _catalog.Count)
        {
            MeshRenderer meshRenderer = _createdObject.GetComponent<MeshRenderer>();

            Collider[] overlapBoxes = Physics.OverlapBox(meshRenderer.bounds.center,
                meshRenderer.bounds.size, _createdObject.transform.rotation, _buildingLayer.value);

            Debug.Log(overlapBoxes.Length);

            if (overlapBoxes.Length == 0)
            {
                _createdObject.AddComponent<MeshCollider>();
                ObjectParentPlacement(position);

                Undo.RegisterCreatedObjectUndo(_createdObject, "Create Building");
            }
        }

        _createdObject = null;
    }

    private void ObjectParentPlacement(Vector3 position)
    {
        _createdObject.transform.position = position;
        _createdObject.transform.parent = _parent.transform;
    }

    private bool CheckInput()
    {
        HandleUtility.AddDefaultControl(0);

        return Event.current.type == EventType.MouseDown && Event.current.button == 0;
    }

    private void RotateObject(Vector3 position, Color color)
    {
        var rotationAngle = 90f;
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.A)
        {
            _createdObject.transform.Rotate(0f, rotationAngle, 0f);
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.D)
        {
            _createdObject.transform.Rotate(0f, -rotationAngle, 0f);
        }

        DrawHandleBox(color, position);
    }

    private void DrawCatalog(List<GUIContent> catalogIcons)
    {
        GUILayout.Label("Buildings");
        _selectedElement = GUILayout.SelectionGrid(_selectedElement, catalogIcons.ToArray(), 4, GUILayout.Width(400), GUILayout.Height(200));
    }

    private List<GUIContent> GetCatalogIcons()
    {
        List<GUIContent> catalogIcons = new List<GUIContent>();

        foreach (var element in _catalog)
        {
            Texture2D texture = AssetPreview.GetAssetPreview(element);
            catalogIcons.Add(new GUIContent(texture));
        }

        return catalogIcons;
    }

    private void RefreshCatalog()
    {
        _catalog.Clear();

        System.IO.Directory.CreateDirectory(_path);
        string[] prefabFiles = System.IO.Directory.GetFiles(_path, "*.prefab");
        foreach (var prefabFile in prefabFiles)
            _catalog.Add(AssetDatabase.LoadAssetAtPath(prefabFile, typeof(GameObject)) as GameObject);
    }
}