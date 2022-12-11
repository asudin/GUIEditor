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
        _parent = (GameObject)EditorGUILayout.ObjectField("Parent", _parent, typeof(GameObject), true);
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
                DrawPointer(contactPoint, Color.red);
                RotateObject();

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

        if (Physics.Raycast(guiRay, out RaycastHit raycastHit))
        {
            contactPoint = raycastHit.point;
            return true;
        }

        return false;
    }

    private void DrawPointer(Vector3 position, Color color)
    {
        if (_createdObject == null)
            _createdObject = Instantiate(_catalog[_selectedElement]);

        ObjectParentPlacement(position);

        DrawHandleBox(color, position);
    }

    private void DrawHandleBox(Color color, Vector3 position)
    {
        Mesh mesh = _createdObject.GetComponentsInChildren<MeshFilter>()[0].sharedMesh;
        Handles.color = color;
        Handles.DrawWireCube(position, mesh.bounds.size);
    }

    private void CreateObject(Vector3 position)
    {
        if (_selectedElement < _catalog.Count)
        {
            _createdObject.AddComponent<MeshCollider>();
            ObjectParentPlacement(position);

            Undo.RegisterCreatedObjectUndo(_createdObject, "Create Building");
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

    private void RotateObject()
    {
        var rotationAngle = 15f;
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.A)
        {
            _createdObject.transform.Rotate(0f, rotationAngle, 0f);
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.D)
        {
            _createdObject.transform.Rotate(0f, -rotationAngle, 0f);
        }
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