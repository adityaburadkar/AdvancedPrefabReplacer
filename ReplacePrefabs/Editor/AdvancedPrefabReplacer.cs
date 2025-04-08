using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AdvancedPrefabReplacer : EditorWindow
{
    private GameObject targetPrefab;

    private bool keepPosition = true;
    private bool keepRotation = true;
    private bool keepScale = true;
    private bool keepLayer = true;
    private bool keepParent = true;
    private bool showPreview = true;

    private List<GameObject> previewObjects = new List<GameObject>();

    [MenuItem("Tools/Advanced Prefab Replacer")]
    public static void ShowWindow()
    {
        GetWindow<AdvancedPrefabReplacer>("Advanced Prefab Replacer");
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        Selection.selectionChanged += Repaint;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        Selection.selectionChanged -= Repaint;
    }

    void OnGUI()
    {
        GUILayout.Label("Prefab Replacement Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        GameObject selected = Selection.activeGameObject;
        if (selected != null)
        {
            EditorGUILayout.LabelField("Selected Prefab Instance:", selected.name);
        }
        else
        {
            EditorGUILayout.LabelField("Select a prefab instance in the scene.");
        }

        targetPrefab = (GameObject)EditorGUILayout.ObjectField("Target Prefab", targetPrefab, typeof(GameObject), false);

        EditorGUILayout.Space();
        GUILayout.Label("Replacement Options", EditorStyles.boldLabel);

        keepPosition = EditorGUILayout.Toggle("Keep Position", keepPosition);
        keepRotation = EditorGUILayout.Toggle("Keep Rotation", keepRotation);
        keepScale = EditorGUILayout.Toggle("Keep Scale", keepScale);
        keepLayer = EditorGUILayout.Toggle("Keep Layer", keepLayer);
        keepParent = EditorGUILayout.Toggle("Keep Parent", keepParent);

        EditorGUILayout.Space();
        showPreview = EditorGUILayout.Toggle("Show Preview in Scene", showPreview);

        EditorGUILayout.Space();

        if (GUILayout.Button("Replace Now"))
        {
            ReplacePrefabs();
        }

        if (showPreview)
        {
            UpdatePreviewObjects();
        }
        else
        {
            previewObjects.Clear();
        }
    }

    void UpdatePreviewObjects()
    {
        previewObjects.Clear();
        GameObject selected = Selection.activeGameObject;

        if (selected == null) return;

        GameObject sourceRoot = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(selected);
        if (sourceRoot == null) return;

        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            GameObject objRoot = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(obj);
            if (objRoot == sourceRoot)
            {
                previewObjects.Add(obj);
            }
        }

        SceneView.RepaintAll();
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (!showPreview || previewObjects == null) return;

        Handles.color = Color.yellow;
        foreach (GameObject obj in previewObjects)
        {
            if (obj != null)
            {
                Renderer rend = obj.GetComponent<Renderer>();
                Vector3 size = rend != null ? rend.bounds.size : Vector3.one;
                Handles.DrawWireCube(obj.transform.position, size);
            }
        }
    }

    void ReplacePrefabs()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null || targetPrefab == null)
        {
            Debug.LogError("Select a prefab instance in the scene and assign a target prefab.");
            return;
        }

        GameObject sourceRoot = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(selected);
        if (sourceRoot == null)
        {
            Debug.LogError("Selected object is not a prefab instance.");
            return;
        }

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int replacedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            GameObject objRoot = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(obj);
            if (objRoot == sourceRoot)
            {
                ReplaceInstance(obj);
                replacedCount++;
            }
        }

        Debug.Log($"Replaced {replacedCount} instance(s) of the prefab.");
    }

    void ReplaceInstance(GameObject original)
    {
        Transform parent = keepParent ? original.transform.parent : null;
        Vector3 position = keepPosition ? original.transform.position : Vector3.zero;
        Quaternion rotation = keepRotation ? original.transform.rotation : Quaternion.identity;
        Vector3 scale = keepScale ? original.transform.localScale : Vector3.one;
        int layer = keepLayer ? original.layer : 0;

        GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(targetPrefab, original.scene);
        newObj.transform.SetParent(parent);
        newObj.transform.position = position;
        newObj.transform.rotation = rotation;
        newObj.transform.localScale = scale;
        newObj.layer = layer;

        Undo.RegisterCreatedObjectUndo(newObj, "Replace Prefab");
        Undo.DestroyObjectImmediate(original);
    }
}