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
        if (Input.GetKeyDown(KeyCode.A))
        {
            Quaternion currentRotation = _createdObject.transform.rotation;
            currentRotation.eulerAngles += new Vector3(0f, -90, 0f);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Quaternion currentRotation = _createdObject.transform.rotation;
            currentRotation.eulerAngles += new Vector3(0f, 90, 0f);
        }

        _parent = (GameObject)EditorGUILayout.ObjectField("Parent", _parent, typeof(GameObject), true);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (_createdObject != null)
        {
            EditorGUILayout.LabelField("Created Object Settings");
            Transform createdTransform = _createdObject.transform;
            createdTransform.position = EditorGUILayout.Vector3Field("Position", createdTransform.position);
            createdTransform.rotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Rotation", createdTransform.rotation.eulerAngles));
            createdTransform.localScale = EditorGUILayout.Vector3Field("Scale", createdTransform.localScale);
        }

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

                if (CheckInput())
                {
                    CreateObject(contactPoint);
                }

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.A)
                {
                    _createdObject.transform.Rotate(0f, 30, 0f);
                }
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.D)
                {
                    _createdObject.transform.Rotate(0f, -30, 0f);
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
        {
            GameObject prefab = _catalog[_selectedElement];
            _createdObject = Instantiate(prefab);
        }
        
        _createdObject.transform.position = position;

        Handles.color = color;
        Handles.DrawWireCube(position, Vector3.one);
    }

    private bool CheckInput()
    {
        HandleUtility.AddDefaultControl(0);

        return Event.current.type == EventType.MouseDown && Event.current.button == 0;
    }

    private void CreateObject(Vector3 position)
    {
        if (_selectedElement < _catalog.Count)
        {
            _createdObject.AddComponent<MeshCollider>();
            _createdObject.transform.position = position;
            _createdObject.transform.parent = _parent.transform;

            Undo.RegisterCreatedObjectUndo(_createdObject, "Create Building");
        }

        _createdObject = null;
    }

    private void DrawCatalog(List<GUIContent> catalogIcons)
    {
        GUILayout.Label("Buildings");
        _selectedElement = GUILayout.SelectionGrid(_selectedElement, catalogIcons.ToArray(), 4, GUILayout.Width(600), GUILayout.Height(300));
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