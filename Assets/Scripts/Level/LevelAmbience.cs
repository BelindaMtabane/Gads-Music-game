using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Per-level lighting and atmosphere (Opera warm, Museum gallery, Club neon).
/// </summary>
public class LevelAmbience : MonoBehaviour
{
    Light _directional;
    Color _baseDirColor;
    float _baseDirIntensity;
    Coroutine _pulseRoutine;

    public void Apply(LevelDefinition def)
    {
        _directional = FindDirectionalLight();
        if (_directional != null)
        {
            _baseDirColor = _directional.color;
            _baseDirIntensity = _directional.intensity;
        }

        RenderSettings.fog = false;

        if (_pulseRoutine != null)
            StopCoroutine(_pulseRoutine);

        switch (def.levelNumber)
        {
            case 1: ApplyOpera(); break;
            case 2: ApplyMuseum(); break;
            case 3: ApplyClub(); break;
        }
    }

    void ApplyOpera()
    {
        if (_directional != null)
        {
            _directional.color = new Color(1f, 0.92f, 0.82f);
            _directional.intensity = 1.15f;
        }
        RenderSettings.ambientLight = new Color(0.28f, 0.22f, 0.24f);
    }

    void ApplyMuseum()
    {
        if (_directional != null)
        {
            _directional.color = new Color(0.85f, 0.88f, 0.95f);
            _directional.intensity = 0.75f;
        }
        RenderSettings.ambientLight = new Color(0.12f, 0.13f, 0.16f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.08f, 0.09f, 0.12f);
        RenderSettings.fogStartDistance = 40f;
        RenderSettings.fogEndDistance = 120f;
    }

    void ApplyClub()
    {
        if (_directional != null)
        {
            _directional.color = new Color(0.72f, 0.76f, 0.95f);
            _directional.intensity = 0.72f;
        }

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.2f, 0.19f, 0.28f);
        RenderSettings.ambientIntensity = 1f;
        RenderSettings.reflectionIntensity = 0.6f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.14f, 0.13f, 0.24f);
        RenderSettings.fogStartDistance = 55f;
        RenderSettings.fogEndDistance = 140f;

        ApplyTwilightCamera();

        _pulseRoutine = StartCoroutine(PulseNeonLights());
    }

    static void ApplyTwilightCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.14f, 0.16f, 0.3f);
    }

    IEnumerator PulseNeonLights()
    {
        while (true)
        {
            float t = Time.time * 2.5f;
            foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional) continue;
                if (!light.name.Contains("Neon")) continue;
                float wave = 0.65f + 0.35f * Mathf.Sin(t + light.transform.position.z * 0.1f);
                light.intensity = 2.8f * wave;
            }
            yield return null;
        }
    }

    static Light FindDirectionalLight()
    {
        foreach (var l in FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l.type == LightType.Directional)
                return l;
        }
        return null;
    }
}
