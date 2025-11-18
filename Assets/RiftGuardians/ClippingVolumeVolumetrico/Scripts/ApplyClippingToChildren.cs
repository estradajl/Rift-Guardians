using UnityEngine;

public class ApplyClippingToChildren : MonoBehaviour
{
    public Shader clippingShader;   // Asigna JohnVR/Lit_Clipping en el inspector

    void Start()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            foreach (var mat in renderer.materials)
            {
                if (mat == null) continue;

                // Guardar propiedades del material original
                Texture baseMap = mat.GetTexture("_BaseMap");
                Color baseColor = mat.GetColor("_BaseColor");

                // Crear nuevo material con el shader de clipping
                Material newMat = new Material(clippingShader);

                // Re-asignar propiedades
                if (baseMap) newMat.SetTexture("_BaseMap", baseMap);
                newMat.SetColor("_BaseColor", baseColor);

                // Aplicar material nuevo
                mat.shader = clippingShader;
                mat.CopyPropertiesFromMaterial(newMat);
            }
        }
    }
}
