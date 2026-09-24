using UnityEngine;

/// <summary>
/// Keeps a beat phase locked to the looping background track.
/// Tempos were measured from the gameplay music files.
/// </summary>
public class MusicBeatClock : MonoBehaviour
{
    public struct TrackTempo
    {
        public string clipName;
        public float bpm;
        public float offsetSeconds;
    }

    public static readonly TrackTempo[] Tracks =
    {
        new TrackTempo { clipName = "Morning_on_the_Plateau", bpm = 92f, offsetSeconds = 0.174f },
        new TrackTempo { clipName = "Sunlight_in_the_Great_Hall", bpm = 89f, offsetSeconds = 0.205f },
        new TrackTempo { clipName = "level2museumSound", bpm = 89f, offsetSeconds = 0.205f },
        new TrackTempo { clipName = "Feet_Against_the_Earth", bpm = 123f, offsetSeconds = 0.279f },
    };

    /// <summary>1 on the beat, then falls back to 0 before the next beat.</summary>
    public static float Pulse { get; private set; }

    public static float CurrentBpm { get; private set; }

    AudioManager _audio;

    void Awake()
    {
        _audio = GetComponent<AudioManager>();
    }

    void Update()
    {
        Pulse = 0f;
        CurrentBpm = 0f;

        if (_audio == null)
            _audio = GetComponent<AudioManager>();
        if (_audio == null)
            return;

        _audio.RepairSingleton();

        var source = _audio.MusicSource;
        if (source == null || !source.isPlaying || !source.loop || source.clip == null)
            return;

        if (!TryGetTempo(source.clip.name, out float bpm, out float offset))
            return;

        CurrentBpm = bpm;
        float time = source.timeSamples / (float)source.clip.frequency;
        float beats = (time - offset) * bpm / 60f;
        float phase = beats - Mathf.Floor(beats);

        const float beatWindow = 0.22f;
        Pulse = phase < beatWindow
            ? Mathf.Sin(phase / beatWindow * Mathf.PI)
            : 0f;
    }

    public static bool TryGetTempo(string clipName, out float bpm, out float offsetSeconds)
    {
        for (int i = 0; i < Tracks.Length; i++)
        {
            if (Tracks[i].clipName != clipName) continue;
            bpm = Tracks[i].bpm;
            offsetSeconds = Tracks[i].offsetSeconds;
            return true;
        }

        bpm = 0f;
        offsetSeconds = 0f;
        return false;
    }
}
