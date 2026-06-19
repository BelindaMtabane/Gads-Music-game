using System.Collections;
using UnityEngine;

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
            _directional.color = new Color(0.55f, 0.65f, 1f);
            _directional.intensity = 0.45f;
        }
        RenderSettings.ambientLight = new Color(0.04f, 0.03f, 0.08f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.02f, 0.01f, 0.06f);
        RenderSettings.fogStartDistance = 25f;
        RenderSettings.fogEndDistance = 90f;
        _pulseRoutine = StartCoroutine(PulseNeonLights());
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
