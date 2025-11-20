using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MaterialAutoAssigner : EditorWindow
{
    // Folder with new materials to assign
    private Object newMaterialsFolder;

    // Mapping from base-map texture name to material
    private Dictionary<string, Material> materialLookup;

    [MenuItem("Tools/Material Auto Assigner")]
    public static void ShowWindow()
    {
        GetWindow<MaterialAutoAssigner>("Material Auto Assigner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Material Assignment Settings", EditorStyles.boldLabel);

        newMaterialsFolder = EditorGUILayout.ObjectField("New Materials Folder:", newMaterialsFolder, typeof(Object), false);

        if (GUILayout.Button("Scan Materials"))
        {
            BuildMaterialLookup();
        }

        if (materialLookup != null)
            EditorGUILayout.LabelField("Loaded Materials: " + materialLookup.Count);

        if (GUILayout.Button("Assign to Selected"))
        {
            AssignToSelectedObjects();
        }
    }

    private void BuildMaterialLookup()
    {
        materialLookup = new Dictionary<string, Material>();

        if (newMaterialsFolder == null)
        {
            Debug.LogWarning("No folder selected.");
            return;
        }

        string path = AssetDatabase.GetAssetPath(newMaterialsFolder);
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { path });

        foreach (string guid in guids)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
            if (!mat) continue;

            if (mat.mainTexture != null)
            {
                string key = mat.mainTexture.name.ToLower();
                if (!materialLookup.ContainsKey(key))
                    materialLookup.Add(key, mat);
            }
        }

        Debug.Log("Material lookup built.");
    }

    private void AssignToSelectedObjects()
    {
        if (materialLookup == null)
        {
            Debug.LogError("Material lookup not built.");
            return;
        }

        foreach (GameObject go in Selection.gameObjects)
        {
            Renderer rend = go.GetComponent<Renderer>();
            if (!rend) continue;

            Material[] mats = rend.sharedMaterials;

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null || mats[i].mainTexture == null) continue;

                string textureKey = mats[i].mainTexture.name.ToLower();

                if (materialLookup.TryGetValue(textureKey, out Material newMat))
                {
                    mats[i] = newMat;
                }
            }

            rend.sharedMaterials = mats;
        }

        Debug.Log("Material assignment completed.");
    }
}
