using UnityEngine;
using UnityEditor;

public class ColorPropertyBatcher : EditorWindow
{
    private Color tintColor = Color.white;

    [MenuItem("Tools/Color Property Batcher")]
    public static void ShowWindow()
    {
        GetWindow<ColorPropertyBatcher>("Color Property Batcher");
    }

    private void OnGUI()
    {
        tintColor = EditorGUILayout.ColorField("Tint Color", tintColor);

        if (GUILayout.Button("Apply to Selected"))
        {
            ApplyColorToSelected();
        }
    }

    private void ApplyColorToSelected()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            Renderer rend = go.GetComponent<Renderer>();
            if (!rend) continue;

            var block = new MaterialPropertyBlock();
            rend.GetPropertyBlock(block);

            block.SetColor("_BaseColor", tintColor); // URP’s Lit shader color property
            rend.SetPropertyBlock(block);
        }

        Debug.Log("Assigned color to selected objects.");
    }
}
