using UnityEngine;
using UnityEditor;

public class RightClickPrefabReplacer
{
    [MenuItem("GameObject/Replace With Selected Prefab", false, 0)]
    private static void ReplaceWithSelectedPrefab(MenuCommand command)
    {
        GameObject selectedSceneObj = command.context as GameObject;
        GameObject selectedPrefab = Selection.activeObject as GameObject;

        if (selectedSceneObj == null || selectedPrefab == null)
        {
            Debug.LogError("Select a scene object and a prefab in the Project.");
            return;
        }

        if (!PrefabUtility.IsPartOfAnyPrefab(selectedPrefab))
        {
            Debug.LogError("Selected Project object is not a prefab.");
            return;
        }

        GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab, selectedSceneObj.scene);
        newObj.transform.position = selectedSceneObj.transform.position;
        newObj.transform.rotation = selectedSceneObj.transform.rotation;
        newObj.transform.localScale = selectedSceneObj.transform.localScale;
        newObj.transform.parent = selectedSceneObj.transform.parent;
        newObj.layer = selectedSceneObj.layer;

        Undo.RegisterCreatedObjectUndo(newObj, "Replace Prefab");
        Undo.DestroyObjectImmediate(selectedSceneObj);
    }

    [MenuItem("GameObject/Replace With Selected Prefab", true)]
    private static bool ValidateReplaceWithSelectedPrefab()
    {
        return Selection.activeObject != null && PrefabUtility.IsPartOfPrefabAsset(Selection.activeObject);
    }
}