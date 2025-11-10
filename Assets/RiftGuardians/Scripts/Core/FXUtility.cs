// Core/FXUtility.cs
using UnityEngine;
using DG.Tweening;

public static class FXUtility
{
    // ==========================================================
    // CONFIGURACIÓN GLOBAL
    // ==========================================================

    // Material unlit cacheado (evita Shader.Find repetidas veces).
    private static Material _unlitColorMat;
    private static Material UnlitColorMat
    {
        get
        {
            if (_unlitColorMat == null)
            {
                var shader = Shader.Find("Unlit/Color");
                _unlitColorMat = new Material(shader);
            }
            return _unlitColorMat;
        }
    }

    // Anti-spam: previene crear 200 luces simultáneamente.
    private static float lastLightPulseTime;
    private const float minPulseInterval = 0.02f; // 50 Hz máx

    // ==========================================================
    // BEAM FX
    // ==========================================================
    public static GameObject SpawnBeam(
        Vector3 from,
        Vector3 to,
        Color color,
        float width = 0.02f,
        float life = 1.2f)
    {
        var go = new GameObject("FX_Beam");
        var lr = go.AddComponent<LineRenderer>();

        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.SetPositions(new[] { from, to });
        lr.startWidth = width;
        lr.endWidth = width * 0.6f;

        // Material por instancia (evitamos que varios beams modifiquen el mismo)
        var mat = new Material(UnlitColorMat);
        mat.color = color;
        lr.material = mat;

        // Fade-out
        DOTween.To(
            () => mat.color,
            c => mat.color = c,
            new Color(color.r, color.g, color.b, 0f),
            life
        ).SetEase(Ease.InQuad);

        Object.Destroy(go, life + 0.1f);

        return go;
    }

    // ==========================================================
    // MATERIAL TWEEN
    // ==========================================================
    public static Tweener TweenMatFloat(
        Renderer r,
        string property,
        float from,
        float to,
        float dur,
        Ease ease = Ease.Linear)
    {
        if (!r || !r.material.HasProperty(property))
        {
            EventBus.Log($"⚠ Material sin propiedad {property} en {r.name}");
            return null;
        }

        var mat = r.material; // instancia segura

        mat.SetFloat(property, from);
        return DOTween
            .To(() => mat.GetFloat(property), v => mat.SetFloat(property, v), to, dur)
            .SetEase(ease);
    }

    // ==========================================================
    // LIGHT PULSE FX (OPTIMIZADO)
    // ==========================================================
    public static void PulseLight(
        Vector3 pos,
        Color color,
        float intensity = 5f,
        float range = 3f,
        float duration = 0.6f)
    {
        // Anti-spam (especialmente útil en VR)
        if (Time.time - lastLightPulseTime < minPulseInterval)
            return;

        lastLightPulseTime = Time.time;

        var go = new GameObject("FX_LightPulse");
        var light = go.AddComponent<Light>();

        go.transform.position = pos;
        light.type = LightType.Point;
        light.color = color;
        light.range = range;
        light.intensity = 0f;

        var seq = DOTween.Sequence();

        seq.Append(
            DOTween.To(() => light.intensity, x => light.intensity = x, intensity, duration * 0.35f)
            .SetEase(Ease.OutCubic)
        );

        seq.Append(
            DOTween.To(() => light.intensity, x => light.intensity = x, 0f, duration * 0.65f)
            .SetEase(Ease.InCubic)
        );

        seq.OnComplete(() => Object.Destroy(go));
    }

    // ==========================================================
    // AUDIO: ONE-SHOT 3D
    // ==========================================================
    public static void PlayOneShot3D(AudioClip clip, Vector3 pos, float volume = 1f)
    {
        if (!clip) return;

        var go = new GameObject("SFX_OneShot");
        var src = go.AddComponent<AudioSource>();

        src.spatialBlend = 1f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.maxDistance = 12f;
        src.volume = volume;

        go.transform.position = pos;
        src.PlayOneShot(clip);

        Object.Destroy(go, clip.length + 0.2f);
    }
}
