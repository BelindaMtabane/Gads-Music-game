using UnityEngine;

/// <summary>
/// Spawns museum display cases (L2) and neon club strips (L3) along the runner lanes.
/// </summary>
public class LevelSetDressing : MonoBehaviour
{
    Transform _player;
    Transform _root;
    int _appliedLevel;

    public void Apply(LevelDefinition def)
    {
        if (def == null || _appliedLevel == def.levelNumber) return;
        _appliedLevel = def.levelNumber;

        if (_root != null)
            Object.Destroy(_root.gameObject);

        _root = new GameObject("LevelSetDressing").transform;

        var playerGo = GameObject.FindGameObjectWithTag("Player");
        _player = playerGo != null ? playerGo.transform : null;
        float startZ = _player != null ? _player.position.z : 0f;

        switch (def.levelNumber)
        {
            case 1:
                BuildOperaHouse(startZ);
                break;
            case 2:
                BuildMuseumDisplays(startZ);
                BuildMuseumLasers(startZ);
                BuildMuseumStrings(startZ);
                BuildMuseumPiano(startZ);
                BuildMuseumEndWall(startZ);
                break;
            case 3:
                BuildClubNeon(startZ);
                BuildClubTrumpetBeams(startZ);
                BuildClubDancers(startZ);
                BuildClubEndWall(startZ);
                break;
        }
    }

    void BuildOperaHouse(float startZ)
    {
        Color crimson = new Color(0.478f, 0.118f, 0.118f);
        Color gold = new Color(0.831f, 0.686f, 0.216f);
        Color midnight = new Color(0.043f, 0.043f, 0.043f);
        Material wallMat = CreateLitMaterial(crimson);
        Material goldMat = CreateLitMaterial(gold, emissive: true);
        Material darkMat = CreateLitMaterial(midnight);

        const float exitDistance = 185f;
        float exitZ = startZ + exitDistance;

        for (float z = startZ + 16f; z < exitZ - 10f; z += 28f)
        {
            foreach (int side in new[] { -1, 1 })
            {
                var practical = new GameObject("OperaPractical").AddComponent<Light>();
                practical.transform.SetParent(_root, false);
                practical.transform.position = new Vector3(side * 10.6f, 3.6f, z);
                practical.type = LightType.Point;
                practical.range = 14f;
                practical.intensity = 1.35f;
                practical.color = new Color(1f, 0.86f, 0.62f);
            }
        }

        BuildOperaExit(exitZ, gold, wallMat, goldMat, darkMat);
        BuildPianoFloor(startZ);
    }

    void BuildPianoFloor(float startZ)
    {
        float footY = 0f;
        if (_player != null)
        {
            var body = _player.GetComponent<CharacterController>();
            footY = body != null
                ? _player.position.y + body.center.y - body.height * 0.5f
                : _player.position.y;
        }

        Material ivory = CreateLitMaterial(new Color(0.94f, 0.92f, 0.87f));
        Material goldKey = CreateLitMaterial(new Color(1f, 0.82f, 0.28f), emissive: true);
        Material redKey = CreateLitMaterial(new Color(0.85f, 0.08f, 0.1f), emissive: true);
        Material gapMat = CreateLitMaterial(new Color(0.08f, 0.07f, 0.07f));

        float z = startZ + 52f;
        for (int phrase = 0; phrase < 2; phrase++)
        {
            int notes = Random.Range(2, 5);
            z = BuildPianoPhrase(footY, z, notes, 8.5f, ivory, goldKey, redKey, gapMat);
            z += 46f;
        }
    }

    float BuildPianoPhrase(float footY, float z, int notes, float rowStep, Material ivory, Material goldKey, Material redKey, Material gapMat)
    {
        const float trackHalf = 12.5f;
        const int keyCount = 4;
        float keyWidth = (trackHalf * 2f) / keyCount;
        const float rowDepth = 3.2f;

        for (int note = 0; note < notes; note++)
        {
            bool[] correct = RandomCorrectKeys();
            for (int i = 0; i < keyCount; i++)
            {
                float minX = -trackHalf + i * keyWidth;
                float centerX = minX + keyWidth * 0.5f;
                bool safe = correct[i];

                var key = GameObject.CreatePrimitive(PrimitiveType.Cube);
                key.name = safe ? "PianoKeySafe" : "PianoKeyJump";
                key.transform.SetParent(_root, false);
                key.transform.position = new Vector3(centerX, footY + 0.035f, z);
                key.transform.localScale = new Vector3(keyWidth - 0.16f, 0.06f, rowDepth);
                ApplyMat(key, ivory);
                DestroyCollider(key);

                var highlight = GameObject.CreatePrimitive(PrimitiveType.Cube);
                highlight.name = safe ? "PianoHighlightRun" : "PianoHighlightJump";
                highlight.transform.SetParent(_root, false);
                highlight.transform.position = new Vector3(centerX, footY + 0.08f, z - rowDepth * 0.28f);
                highlight.transform.localScale = new Vector3(keyWidth - 0.28f, 0.05f, rowDepth * 0.34f);
                ApplyMat(highlight, safe ? goldKey : redKey);
                DestroyCollider(highlight);

                var trigger = new GameObject("PianoKeyTrigger");
                trigger.transform.SetParent(_root, false);
                trigger.transform.position = new Vector3(centerX, footY + 0.45f, z);
                var box = trigger.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = new Vector3(keyWidth, 0.9f, rowDepth);
                var piano = trigger.AddComponent<PianoKey>();
                piano.correct = safe;
                piano.minX = minX;
                piano.maxX = minX + keyWidth;

                if (i < keyCount - 1)
                {
                    var gap = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    gap.name = "PianoGap";
                    gap.transform.SetParent(_root, false);
                    gap.transform.position = new Vector3(minX + keyWidth, footY + 0.02f, z);
                    gap.transform.localScale = new Vector3(0.12f, 0.04f, rowDepth);
                    ApplyMat(gap, gapMat);
                    DestroyCollider(gap);
                }
            }

            z += rowStep;
        }

        return z;
    }

    static bool[] RandomCorrectKeys()
    {
        var correct = new bool[4];
        int safeCount = Random.Range(1, 4);
        while (safeCount > 0)
        {
            int index = Random.Range(0, 4);
            if (correct[index])
                continue;
            correct[index] = true;
            safeCount--;
        }
        return correct;
    }

    void BuildOperaExit(float wallZ, Color gold, Material curtainMat, Material goldMat, Material darkMat)
    {

        var backing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backing.name = "OperaExitWall";
        backing.transform.SetParent(_root, false);
        backing.transform.position = new Vector3(0f, 4.5f, wallZ);
        backing.transform.localScale = new Vector3(24f, 9f, 0.6f);
        ApplyMat(backing, darkMat);
        DestroyCollider(backing);

        Transform leftCurtain = null;
        Transform rightCurtain = null;
        foreach (int side in new[] { -1, 1 })
        {
            var curtain = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curtain.name = $"OperaExitCurtain_{side}";
            curtain.transform.SetParent(_root, false);
            curtain.transform.position = new Vector3(side * 7.2f, 4.2f, wallZ - 0.2f);
            curtain.transform.localScale = new Vector3(6.2f, 8.2f, 0.25f);
            ApplyMat(curtain, curtainMat);
            DestroyCollider(curtain);
            if (side < 0)
                leftCurtain = curtain.transform;
            else
                rightCurtain = curtain.transform;

            var jamb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            jamb.name = $"OperaExitJamb_{side}";
            jamb.transform.SetParent(_root, false);
            jamb.transform.position = new Vector3(side * 3.6f, 4.2f, wallZ - 0.35f);
            jamb.transform.localScale = new Vector3(0.35f, 8.4f, 0.35f);
            ApplyMat(jamb, goldMat);
            DestroyCollider(jamb);
        }

        var lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lintel.name = "OperaExitLintel";
        lintel.transform.SetParent(_root, false);
        lintel.transform.position = new Vector3(0f, 8.5f, wallZ - 0.35f);
        lintel.transform.localScale = new Vector3(7.6f, 0.4f, 0.4f);
        ApplyMat(lintel, goldMat);
        DestroyCollider(lintel);

        var threshold = GameObject.CreatePrimitive(PrimitiveType.Cube);
        threshold.name = "OperaExitThreshold";
        threshold.transform.SetParent(_root, false);
        threshold.transform.position = new Vector3(0f, 0.08f, wallZ - 4.2f);
        threshold.transform.localScale = new Vector3(7.2f, 0.08f, 1.2f);
        ApplyMat(threshold, goldMat);
        DestroyCollider(threshold);

        var spotlight = new GameObject("OperaExitLight").AddComponent<Light>();
        spotlight.transform.SetParent(_root, false);
        spotlight.transform.position = new Vector3(0f, 8.2f, wallZ - 8f);
        spotlight.type = LightType.Spot;
        spotlight.range = 24f;
        spotlight.spotAngle = 42f;
        spotlight.intensity = 3.2f;
        spotlight.color = gold;
        spotlight.transform.rotation = Quaternion.LookRotation(new Vector3(0f, -0.45f, 1f));

        var door = new GameObject("OperaCurtainDoor");
        door.transform.SetParent(_root, false);
        door.AddComponent<OpeningCurtainDoor>().Setup(leftCurtain, rightCurtain, backing);
    }

    void BuildMuseumLasers(float startZ)
    {
        Material beamMat = CreateLitMaterial(new Color(1f, 0.05f, 0.08f), emissive: true);
        Material postMat = CreateLitMaterial(new Color(0.22f, 0.22f, 0.24f));

        float footY = 0f;
        if (_player != null)
        {
            var body = _player.GetComponent<CharacterController>();
            footY = body != null
                ? _player.position.y + body.center.y - body.height * 0.5f
                : _player.position.y;
        }

        // Hits a running player. A normal jump lifts the body clear of the beam.
        float beamY = footY + 1.05f;

        for (float z = startZ + 34f; z < startZ + 175f; z += 28f)
        {
            foreach (int side in new[] { -1, 1 })
            {
                var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.name = "MuseumLaserPost";
                post.transform.SetParent(_root, false);
                post.transform.position = new Vector3(side * 12.15f, footY + 1.35f, z);
                post.transform.localScale = new Vector3(0.28f, 2.7f, 0.28f);
                ApplyMat(post, postMat);
                DestroyCollider(post);
            }

            var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "MuseumLaser";
            beam.tag = "HealthDEC";
            beam.transform.SetParent(_root, false);
            beam.transform.position = new Vector3(0f, beamY, z);
            beam.transform.localScale = new Vector3(24f, 0.2f, 0.42f);
            ApplyMat(beam, beamMat);

            var hurt = beam.AddComponent<HealthDecreaseObstacle>();
            hurt.damage = 20;
            hurt.vibeDamage = 8;
            hurt.instantKill = false;
            GameplayCollisionUtility.ConfigureObject(beam);

            var glow = new GameObject("MuseumLaserGlow").AddComponent<Light>();
            glow.transform.SetParent(beam.transform, false);
            glow.type = LightType.Point;
            glow.color = new Color(1f, 0.12f, 0.16f);
            glow.range = 7f;
            glow.intensity = 2.4f;
        }
    }

    void BuildMuseumStrings(float startZ)
    {
        float footY = MuseumFootY();
        Material stringMat = CreateLitMaterial(new Color(0.82f, 0.78f, 0.62f), emissive: true);
        Material postMat = CreateLitMaterial(new Color(0.16f, 0.13f, 0.1f));

        // Bottom sits above a G-dodge. Top stays in the way of a jump.
        const float bottom = 1.5f;
        const float top = 3.6f;
        float centerY = footY + (bottom + top) * 0.5f;
        float height = top - bottom;

        float[] offsets = { 48f, 104f, 160f };
        for (int s = 0; s < offsets.Length; s++)
        {
            float z = startZ + offsets[s];

            var band = GameObject.CreatePrimitive(PrimitiveType.Cube);
            band.name = "MuseumGuitarStrings";
            band.tag = "HealthDEC";
            band.transform.SetParent(_root, false);
            band.transform.position = new Vector3(0f, centerY, z);
            band.transform.localScale = new Vector3(25f, height, 0.18f);
            var bandRenderer = band.GetComponent<MeshRenderer>();
            if (bandRenderer != null)
                bandRenderer.enabled = false;

            var hurt = band.AddComponent<HealthDecreaseObstacle>();
            hurt.damage = 20;
            hurt.vibeDamage = 8;
            hurt.instantKill = false;
            GameplayCollisionUtility.ConfigureObject(band);

            for (int line = 0; line < 5; line++)
            {
                float y = footY + bottom + 0.12f + line * 0.42f;
                var wire = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wire.name = "GuitarString";
                wire.transform.SetParent(band.transform, false);
                wire.transform.localPosition = new Vector3(0f, (y - centerY) / height, 0f);
                wire.transform.localScale = new Vector3(1f, 0.035f / height, 2.2f);
                ApplyMat(wire, stringMat);
                DestroyCollider(wire);
            }

            foreach (int side in new[] { -1, 1 })
            {
                var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
                post.name = "GuitarStringPost";
                post.transform.SetParent(_root, false);
                post.transform.position = new Vector3(side * 12.35f, footY + top * 0.5f, z);
                post.transform.localScale = new Vector3(0.22f, top, 0.22f);
                ApplyMat(post, postMat);
                DestroyCollider(post);
            }
        }
    }

    void BuildMuseumPiano(float startZ)
    {
        float footY = MuseumFootY();
        Material ivory = CreateLitMaterial(new Color(0.94f, 0.92f, 0.87f));
        Material goldKey = CreateLitMaterial(new Color(1f, 0.82f, 0.28f), emissive: true);
        Material redKey = CreateLitMaterial(new Color(0.85f, 0.08f, 0.1f), emissive: true);
        Material gapMat = CreateLitMaterial(new Color(0.08f, 0.07f, 0.07f));

        BuildPianoPhrase(footY, startZ + 68f, Random.Range(2, 5), 5.5f, ivory, goldKey, redKey, gapMat);
        BuildPianoPhrase(footY, startZ + 122f, Random.Range(2, 5), 5.5f, ivory, goldKey, redKey, gapMat);
    }

    float MuseumFootY()
    {
        if (_player == null)
            return 0f;
        var body = _player.GetComponent<CharacterController>();
        return body != null
            ? _player.position.y + body.center.y - body.height * 0.5f
            : _player.position.y;
    }

    void BuildMuseumDisplays(float startZ)
    {
        Material pedestalMat = CreateLitMaterial(new Color(0.35f, 0.32f, 0.28f));
        Material glassMat = CreateLitMaterial(new Color(0.55f, 0.65f, 0.75f, 0.35f));
        Material goldMat = CreateLitMaterial(new Color(0.85f, 0.72f, 0.25f));

        for (int i = 0; i < 12; i++)
        {
            float z = startZ + 12f + i * 16f;
            float side = (i % 2 == 0) ? -1f : 1f;
            float x = side * Random.Range(7.5f, 9.5f);

            var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestal.name = $"MuseumPedestal_{i}";
            pedestal.transform.SetParent(_root, false);
            pedestal.transform.position = new Vector3(x, 0.55f, z);
            pedestal.transform.localScale = new Vector3(1.8f, 1.1f, 1.4f);
            ApplyMat(pedestal, pedestalMat);
            DestroyCollider(pedestal);

            var glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            glass.name = "DisplayCase";
            glass.transform.SetParent(pedestal.transform, false);
            glass.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            glass.transform.localScale = new Vector3(0.92f, 1.1f, 0.92f);
            ApplyMat(glass, glassMat);
            DestroyCollider(glass);

            var artifact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            artifact.name = "DisplayArtifact";
            artifact.transform.SetParent(pedestal.transform, false);
            artifact.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            artifact.transform.localScale = Vector3.one * 0.45f;
            ApplyMat(artifact, goldMat);
            DestroyCollider(artifact);

            var spotlight = new GameObject("Spot").AddComponent<Light>();
            spotlight.transform.SetParent(pedestal.transform, false);
            spotlight.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            spotlight.type = LightType.Spot;
            spotlight.range = 6f;
            spotlight.spotAngle = 45f;
            spotlight.intensity = 1.8f;
            spotlight.color = new Color(1f, 0.95f, 0.8f);
            spotlight.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }

    void BuildClubNeon(float startZ)
    {
        Color[] neonColors =
        {
            new Color(1f, 0.1f, 0.85f),
            new Color(0.1f, 0.95f, 1f),
            new Color(0.55f, 0.15f, 1f),
            new Color(1f, 0.95f, 0.2f)
        };

        for (int i = 0; i < 16; i++)
        {
            float z = startZ + 8f + i * 14f;
            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * Random.Range(8f, 10.5f);
                Color c = neonColors[i % neonColors.Length];

                var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                strip.name = $"NeonStrip_{i}_{side}";
                strip.transform.SetParent(_root, false);
                strip.transform.position = new Vector3(x, Random.Range(2f, 4.5f), z);
                strip.transform.localScale = new Vector3(0.15f, Random.Range(2.5f, 4f), 0.15f);
                ApplyMat(strip, CreateLitMaterial(c, emissive: true));
                DestroyCollider(strip);

                var light = new GameObject("NeonLight").AddComponent<Light>();
                light.transform.SetParent(strip.transform, false);
                light.transform.localPosition = Vector3.zero;
                light.type = LightType.Point;
                light.range = 8f;
                light.intensity = 2.8f;
                light.color = c;
            }
        }
    }

    void BuildClubTrumpetBeams(float startZ)
    {
        float footY = MuseumFootY();
        Material gold = CreateLitMaterial(new Color(0.9f, 0.72f, 0.22f), emissive: true);
        Material noteA = CreateLitMaterial(new Color(1f, 0.15f, 0.75f), emissive: true);
        Material noteB = CreateLitMaterial(new Color(0.2f, 0.95f, 1f), emissive: true);

        const float bottom = 1.5f;
        const float top = 3.6f;
        float[] offsets = { 40f, 92f, 144f, 196f };
        for (int i = 0; i < offsets.Length; i++)
        {
            float z = startZ + offsets[i];
            int side = i % 2 == 0 ? -1 : 1;
            BuildSideTrumpet(new Vector3(side * 11.2f, footY + 1.7f, z), side, gold);

            var beam = new GameObject("TrumpetNoteBeam");
            beam.transform.SetParent(_root, false);
            beam.transform.position = new Vector3(0f, footY + (bottom + top) * 0.5f, z);
            var box = beam.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(22f, top - bottom, 1.1f);
            beam.AddComponent<NoteBeam>();
            GameplayCollisionUtility.EnsureKinematicRigidbody(beam);

            for (float x = -9.5f; x <= 9.5f; x += 1.35f)
            {
                float y = footY + bottom + 0.35f + ((Mathf.RoundToInt(x * 2f) & 1) == 0 ? 0.15f : 0.85f);
                BuildMusicNote(new Vector3(x, y, z), (Mathf.RoundToInt(x) & 1) == 0 ? noteA : noteB);
            }
        }
    }

    void BuildSideTrumpet(Vector3 position, int side, Material gold)
    {
        var root = new GameObject("ClubTrumpet").transform;
        root.SetParent(_root, false);
        root.position = position;
        root.rotation = Quaternion.Euler(-8f, side > 0 ? -90f : 90f, 0f);

        AddClubPart(PrimitiveType.Cylinder, root, new Vector3(0f, 0f, 0.35f), new Vector3(0.12f, 0.38f, 0.12f), Quaternion.Euler(90f, 0f, 0f), gold);
        AddClubPart(PrimitiveType.Sphere, root, new Vector3(0f, 0f, 0.85f), new Vector3(0.42f, 0.42f, 0.28f), Quaternion.identity, gold);
        AddClubPart(PrimitiveType.Cylinder, root, new Vector3(0f, 0.16f, 0.2f), new Vector3(0.05f, 0.1f, 0.05f), Quaternion.identity, gold);
    }

    void BuildMusicNote(Vector3 position, Material mat)
    {
        var note = new GameObject("MusicNote").transform;
        note.SetParent(_root, false);
        note.position = position;
        AddClubPart(PrimitiveType.Sphere, note, new Vector3(0.08f, 0f, 0f), new Vector3(0.28f, 0.22f, 0.12f), Quaternion.identity, mat);
        AddClubPart(PrimitiveType.Cube, note, new Vector3(0.2f, 0.28f, 0f), new Vector3(0.05f, 0.55f, 0.05f), Quaternion.identity, mat);
    }

    void BuildClubDancers(float startZ)
    {
        float footY = MuseumFootY();
        Color[] shirts =
        {
            new Color(1f, 0.15f, 0.7f),
            new Color(0.15f, 0.85f, 1f),
            new Color(0.7f, 0.3f, 1f),
            new Color(1f, 0.85f, 0.2f)
        };
        Material skin = CreateLitMaterial(new Color(0.62f, 0.45f, 0.34f));
        float[] lanes = { -6f, 6f, 0f };
        float[] offsets = { 58f, 118f, 176f };

        for (int g = 0; g < lanes.Length; g++)
        {
            float z = startZ + offsets[g];
            var group = new GameObject("DanceCrowd");
            group.transform.SetParent(_root, false);
            group.transform.position = new Vector3(lanes[g], footY + 0.9f, z);
            var box = group.AddComponent<BoxCollider>();
            box.size = new Vector3(4.4f, 1.8f, 3.6f);
            group.AddComponent<DanceCrowd>();

            for (int p = 0; p < 5; p++)
            {
                float px = (p - 2) * 0.75f;
                float pz = (p % 2 == 0 ? -0.55f : 0.45f);
                var person = new GameObject("Dancer");
                person.transform.SetParent(group.transform, false);
                person.transform.localPosition = new Vector3(px, -0.9f, pz);
                Material shirt = CreateLitMaterial(shirts[(g + p) % shirts.Length], emissive: true);
                AddClubPart(PrimitiveType.Capsule, person.transform, new Vector3(0f, 0.85f, 0f), new Vector3(0.38f, 0.42f, 0.38f), Quaternion.identity, shirt);
                AddClubPart(PrimitiveType.Sphere, person.transform, new Vector3(0f, 1.55f, 0f), new Vector3(0.32f, 0.32f, 0.32f), Quaternion.identity, skin);
                person.AddComponent<DancerBob>();
            }
        }
    }

    static void AddClubPart(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 scale, Quaternion rotation, Material mat)
    {
        var part = GameObject.CreatePrimitive(type);
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPos;
        part.transform.localRotation = rotation;
        part.transform.localScale = scale;
        ApplyMat(part, mat);
        DestroyCollider(part);
    }

    // ── End walls ─────────────────────────────────────────────────────────────

    /// <summary>
    /// L2 Museum: grand marble gate placed far ahead as a finish-line landmark.
    /// No physics — player runs through it; purely atmospheric.
    /// </summary>
    void BuildMuseumEndWall(float startZ)
    {
        float wallZ = startZ + 230f;

        Material marbleMat = CreateLitMaterial(new Color(0.78f, 0.75f, 0.70f));
        Material goldMat   = CreateLitMaterial(new Color(0.85f, 0.72f, 0.25f));

        // Main slab
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "MuseumEndWall";
        wall.transform.SetParent(_root, false);
        wall.transform.position = new Vector3(0f, 5f, wallZ);
        wall.transform.localScale = new Vector3(22f, 10f, 0.8f);
        ApplyMat(wall, marbleMat);
        DestroyCollider(wall);

        // Gold top trim
        var trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trim.name = "MuseumEndTrim";
        trim.transform.SetParent(_root, false);
        trim.transform.position = new Vector3(0f, 10.6f, wallZ);
        trim.transform.localScale = new Vector3(22f, 1.2f, 1.0f);
        ApplyMat(trim, goldMat);
        DestroyCollider(trim);

        // Two marble columns flanking the doorway gap
        foreach (int side in new[] { -1, 1 })
        {
            var col = GameObject.CreatePrimitive(PrimitiveType.Cube);
            col.name = $"MuseumColumn_{(side < 0 ? "L" : "R")}";
            col.transform.SetParent(_root, false);
            col.transform.position = new Vector3(side * 5f, 5f, wallZ - 0.05f);
            col.transform.localScale = new Vector3(1.6f, 10f, 1.0f);
            ApplyMat(col, marbleMat);
            DestroyCollider(col);
        }

        // Warm spotlight to draw the eye
        var spotlight = new GameObject("MuseumEndLight").AddComponent<Light>();
        spotlight.transform.SetParent(_root, false);
        spotlight.transform.position = new Vector3(0f, 12f, wallZ - 3f);
        spotlight.type = LightType.Spot;
        spotlight.range = 20f;
        spotlight.spotAngle = 55f;
        spotlight.intensity = 2.2f;
        spotlight.color = new Color(1f, 0.95f, 0.80f);
        spotlight.transform.localRotation = Quaternion.Euler(60f, 180f, 0f);
    }

    /// <summary>
    /// L3 Underground Club: neon portal gate at the far end of the track.
    /// No physics — purely atmospheric.
    /// </summary>
    void BuildClubEndWall(float startZ)
    {
        float wallZ = startZ + 260f;

        Color purple = new Color(0.55f, 0.15f, 1f);
        Color cyan   = new Color(0.1f,  0.95f, 1f);
        Color pink   = new Color(1f,    0.1f,  0.85f);

        // Dark backing wall
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "ClubEndWall";
        wall.transform.SetParent(_root, false);
        wall.transform.position = new Vector3(0f, 5f, wallZ);
        wall.transform.localScale = new Vector3(22f, 10f, 0.8f);
        ApplyMat(wall, CreateLitMaterial(new Color(0.05f, 0.02f, 0.08f)));
        DestroyCollider(wall);

        // Neon top bar (cyan)
        var topBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topBar.name = "ClubDoorTop";
        topBar.transform.SetParent(_root, false);
        topBar.transform.position = new Vector3(0f, 9.6f, wallZ - 0.15f);
        topBar.transform.localScale = new Vector3(9.2f, 0.45f, 0.35f);
        ApplyMat(topBar, CreateLitMaterial(cyan, emissive: true));
        DestroyCollider(topBar);

        // Neon side pillars (pink)
        foreach (int side in new[] { -1, 1 })
        {
            var sideBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sideBar.name = $"ClubDoorSide_{side}";
            sideBar.transform.SetParent(_root, false);
            sideBar.transform.position = new Vector3(side * 4.6f, 4.8f, wallZ - 0.15f);
            sideBar.transform.localScale = new Vector3(0.45f, 9.6f, 0.35f);
            ApplyMat(sideBar, CreateLitMaterial(pink, emissive: true));
            DestroyCollider(sideBar);
        }

        // Wide neon strip across the top of the whole wall (purple)
        var topStrip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topStrip.name = "ClubTopStrip";
        topStrip.transform.SetParent(_root, false);
        topStrip.transform.position = new Vector3(0f, 10.5f, wallZ - 0.1f);
        topStrip.transform.localScale = new Vector3(22f, 0.3f, 0.5f);
        ApplyMat(topStrip, CreateLitMaterial(purple, emissive: true));
        DestroyCollider(topStrip);

        // Atmospheric point light
        var light = new GameObject("ClubEndLight").AddComponent<Light>();
        light.transform.SetParent(_root, false);
        light.transform.position = new Vector3(0f, 5f, wallZ - 4f);
        light.type = LightType.Point;
        light.range = 18f;
        light.intensity = 3.5f;
        light.color = purple;
    }

    static Material CreateLitMaterial(Color color, bool emissive = false)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit")
                     ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                     ?? Shader.Find("Standard");
        var mat = new Material(shader);
        if (color.a < 1f)
        {
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend", 0f);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = 3000;
        }
        mat.color = color;
        if (emissive)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2.2f);
        }
        return mat;
    }

    static void ApplyMat(GameObject go, Material mat)
    {
        var r = go.GetComponent<Renderer>();
        if (r != null) r.sharedMaterial = mat;
    }

    static void DestroyCollider(GameObject go)
    {
        var c = go.GetComponent<Collider>();
        if (c != null) Object.Destroy(c);
    }
}
