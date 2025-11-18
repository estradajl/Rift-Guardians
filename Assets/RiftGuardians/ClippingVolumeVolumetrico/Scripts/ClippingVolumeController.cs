using UnityEngine;

public class ClippingVolumeController : MonoBehaviour
{
    public Transform volume;      // El volumen que quieres usar (cube, sphere, empty, etc.)
    public int clipMode = 0;      // 0 = Sphere, 1 = Box, 2 = Hemisphere

    void Update()
    {
        if (!volume) return;

        // Centro = posición del transform
        Vector3 center = volume.position;

        // Tamaño = escala del transform (convertido a “radio” o semidimensiones)
        Vector3 size = volume.lossyScale * 0.5f;

        Shader.SetGlobalVector("_ClipCenter", center);
        Shader.SetGlobalVector("_ClipSize", size);
        Shader.SetGlobalFloat("_ClipMode", clipMode);
    }
}
