using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
// taken from http://answers.unity3d.com/questions/1073094/custom-inspector-layer-mask-variable.html

public class EditorTools {

    static List<string> layers;
    static string[] layerNames;

    // not working
    public static LayerMask LayerMaskField(string label, LayerMask layerMask) {
        List<string> layers = new List<string>();
        List<int> layerNumbers = new List<int>();

        for (int i = 0; i < 32; i++) {
            string layerName = LayerMask.LayerToName(i);
            if (layerName != "") {
                layers.Add(layerName);
                layerNumbers.Add(i);
            }
        }
        int maskWithoutEmpty = 0;
        for (int i = 0; i < layerNumbers.Count; i++) {
            if (((1 << layerNumbers[i]) & layerMask.value) > 0)
                maskWithoutEmpty |= (1 << i);
        }
        maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());
        int mask = 0;
        for (int i = 0; i < layerNumbers.Count; i++) {
            if ((maskWithoutEmpty & (1 << i)) > 0)
                mask |= (1 << layerNumbers[i]);
        }
        layerMask.value = mask;
        return layerMask;
    }
    public static void CreatePrefab(GameObject prefab) {
        // Create some asset folders.
        AssetDatabase.CreateFolder("Assets/Meshes", "MyMeshes");
        string mapPath = "Assets/Maps";
        if (!AssetDatabase.IsValidFolder(mapPath)) {
            mapPath = AssetDatabase.CreateFolder("Assets", "Maps");
            mapPath = AssetDatabase.GUIDToAssetPath(mapPath);
        }
        // The paths to the mesh/prefab assets.
		string prefabPath = mapPath + "/" + prefab.name + ".prefab";

        //AssetDatabase.DeleteAsset(prefabPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        // Save the transform's GameObject as a prefab asset.
        PrefabUtility.CreatePrefab(prefabPath, prefab, ReplacePrefabOptions.Default);

    }
}