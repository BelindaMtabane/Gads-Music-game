using UnityEngine;

/// <summary>
/// Brightens obstacles and pickups on the background-music beat.
/// Scale only changes on meshes that have no collider, so the hitbox stays still.
/// </summary>
public class BeatPulseVisual : MonoBehaviour
{
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    struct MeshPulse
    {
        public Renderer renderer;
        public Color baseColor;
        public Color emission;
        public bool hasEmission;
        public Transform scaleTarget;
        public Vector3 baseScale;
    }

    MeshPulse[] _meshes;
    Light[] _lights;
    float[] _lightBase;
    MaterialPropertyBlock _block;

    void Awake()
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        _meshes = new MeshPulse[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            var mat = renderer.sharedMaterial;
            Color baseColor = Color.white;
            Color emission = Color.black;
            bool hasEmission = false;
            if (mat != null)
            {
                if (mat.HasProperty(BaseColorId)) baseColor = mat.GetColor(BaseColorId);
                else if (mat.HasProperty(ColorId)) baseColor = mat.GetColor(ColorId);
                if (mat.HasProperty(EmissionId) && mat.IsKeywordEnabled("_EMISSION"))
                {
                    emission = mat.GetColor(EmissionId);
                    hasEmission = true;
                }
            }

            Transform scaleTarget = null;
            if (renderer.transform.GetComponent<Collider>() == null
                && renderer.transform.GetComponentInChildren<Collider>() == null)
            {
                scaleTarget = renderer.transform;
            }

            _meshes[i] = new MeshPulse
            {
                renderer = renderer,
                baseColor = baseColor,
                emission = emission,
                hasEmission = hasEmission,
                scaleTarget = scaleTarget,
                baseScale = scaleTarget != null ? scaleTarget.localScale : Vector3.one
            };
        }

        _lights = GetComponentsInChildren<Light>(true);
        _lightBase = new float[_lights.Length];
        for (int i = 0; i < _lights.Length; i++)
            _lightBase[i] = _lights[i].intensity;

        _block = new MaterialPropertyBlock();
    }

    void LateUpdate()
    {
        float pulse = MusicBeatClock.Pulse;
        const float scaleAmount = 0.16f;

        for (int i = 0; i < _meshes.Length; i++)
        {
            var mesh = _meshes[i];
            if (mesh.renderer == null) continue;

            Color bright = Color.Lerp(mesh.baseColor, Color.white, 0.4f);
            Color color = Color.Lerp(mesh.baseColor, bright * 1.65f, pulse);
            color.a = mesh.baseColor.a;

            mesh.renderer.GetPropertyBlock(_block);
            _block.SetColor(BaseColorId, color);
            _block.SetColor(ColorId, color);
            if (mesh.hasEmission)
                _block.SetColor(EmissionId, mesh.emission * (1f + pulse * 1.4f));
            mesh.renderer.SetPropertyBlock(_block);

            if (mesh.scaleTarget != null)
                mesh.scaleTarget.localScale = mesh.baseScale * (1f + pulse * scaleAmount);
        }

        for (int i = 0; i < _lights.Length; i++)
        {
            if (_lights[i] == null || _lights[i].type == LightType.Directional) continue;
            _lights[i].intensity = _lightBase[i] * (1f + pulse * 1.2f);
        }
    }

    public static void Attach(GameObject go)
    {
        if (go == null || !IsObstacleOrPickup(go) || IsInstrument(go)) return;

        var host = PulseHost(go.transform);
        if (IsPlayer(host)) return;
        var hostObject = host.gameObject;
        if (hostObject.GetComponent<BeatPulseVisual>() == null)
            hostObject.AddComponent<BeatPulseVisual>();
    }

    static bool IsPlayer(Transform t)
    {
        while (t != null)
        {
            if (t.GetComponent<PlayerMovement>() != null) return true;
            t = t.parent;
        }
        return false;
    }

    public static void BindLoadedObjects()
    {
        BindMarkers<HealthDecreaseObstacle>();
        BindMarkers<SlowDownObstacle>();
        BindMarkers<CurtainObstacle>();
        BindMarkers<HealthPickup>();
        BindMarkers<SpeedBoostPickup>();
        BindMarkers<JumpBoostPickup>();
        BindMarkers<SneakPickup>();

        BindTag("HealthDEC");
        BindTag("HealthINC");
        BindTag("Artifact");
        BindTag("JumpBoost");
        BindTag("Sneak");
        BindTag("Speed");
        BindTag("SlowDown");
        BindTag("Curtain");
    }

    static bool IsInstrument(GameObject go)
    {
        if (go.GetComponentInParent<MusicInstrument>() != null) return true;
        if (go.GetComponentInChildren<MusicInstrument>(true) != null) return true;

        var transforms = go.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].CompareTag("Artifact")) return true;
            if (NameIsInstrument(transforms[i].name)) return true;
        }

        return NameIsInstrument(go.name);
    }

    static bool NameIsInstrument(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;
        return name.IndexOf("drum", System.StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("flute", System.StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("trumpet", System.StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("instrument", System.StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("musicnote", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    static void BindMarkers<T>() where T : Component
    {
        var found = Object.FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < found.Length; i++)
            Attach(found[i].gameObject);
    }

    static void BindTag(string tag)
    {
        GameObject[] found;
        try
        {
            found = GameObject.FindGameObjectsWithTag(tag);
        }
        catch (UnityException)
        {
            return;
        }

        for (int i = 0; i < found.Length; i++)
            Attach(found[i]);
    }

    static bool IsObstacleOrPickup(GameObject go)
    {
        if (go.GetComponentInChildren<HealthDecreaseObstacle>(true) != null) return true;
        if (go.GetComponentInChildren<SlowDownObstacle>(true) != null) return true;
        if (go.GetComponentInChildren<CurtainObstacle>(true) != null) return true;
        if (go.GetComponentInChildren<HealthPickup>(true) != null) return true;
        if (go.GetComponentInChildren<SpeedBoostPickup>(true) != null) return true;
        if (go.GetComponentInChildren<JumpBoostPickup>(true) != null) return true;
        if (go.GetComponentInChildren<SneakPickup>(true) != null) return true;

        var transforms = go.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            string tag = transforms[i].tag;
            if (tag == "Untagged") continue;
            if (GameplayCollisionUtility.IsPickupTag(tag)) return true;
            if (GameplayCollisionUtility.IsSolidObstacleTag(tag)) return true;
            if (tag == "Curtain") return true;
        }

        return false;
    }

    static Transform PulseHost(Transform start)
    {
        Transform host = start;
        while (host.parent != null && !IsTrack(host.parent))
            host = host.parent;
        return host;
    }

    static bool IsTrack(Transform t)
    {
        if (t.GetComponent<GroundSegmentLifetime>() != null) return true;
        string n = t.name;
        if (n.IndexOf("Ground", System.StringComparison.OrdinalIgnoreCase) >= 0) return true;
        if (n == "LevelSetDressing") return true;
        return false;
    }
}
