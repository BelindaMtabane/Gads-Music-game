using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Generates short pickup WAV files for distinct in-game collect sounds.
/// </summary>
public static class PickupSfxGenerator
{
    public const string OutputFolder = "Assets/Audios/Pickups";

    [MenuItem("Tools/Generate Pickup SFX")]
    public static void GenerateAll()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Audios"))
            AssetDatabase.CreateFolder("Assets", "Audios");
        if (!AssetDatabase.IsValidFolder(OutputFolder))
            AssetDatabase.CreateFolder("Assets/Audios", "Pickups");

        WriteTone($"{OutputFolder}/pickup_artifact.wav", 880f, 0.18f, 0.55f);
        WriteTone($"{OutputFolder}/pickup_health.wav", 523f, 0.14f, 0.5f);
        WriteTone($"{OutputFolder}/pickup_sneak.wav", 280f, 0.22f, 0.4f);
        WriteTone($"{OutputFolder}/pickup_speed.wav", 1180f, 0.12f, 0.55f);
        WriteTone($"{OutputFolder}/pickup_jump.wav", 660f, 0.13f, 0.5f);
        WriteDualTone($"{OutputFolder}/obstacle_hit.wav", 190f, 95f, 0.17f, 0.62f);
        WriteSweep($"{OutputFolder}/obstacle_slowdown.wav", 640f, 220f, 0.24f, 0.52f);

        AssetDatabase.Refresh();
        Debug.Log("[PickupSfxGenerator] Pickup and obstacle WAV files created in Assets/Audios/Pickups.");
    }

    private static void WriteTone(string assetPath, float frequency, float durationSec, float volume)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSec));
        var samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = Mathf.Clamp01(1f - (t / durationSec));
            envelope *= envelope;
            float sample = Mathf.Sin(2f * Mathf.PI * frequency * t) * volume * envelope;
            samples[i] = (short)Mathf.Clamp(sample * short.MaxValue, short.MinValue, short.MaxValue);
        }

        File.WriteAllBytes(assetPath, BuildWav(samples, sampleRate));
    }

    static void WriteDualTone(string assetPath, float freqA, float freqB, float durationSec, float volume)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSec));
        var samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = Mathf.Clamp01(1f - (t / durationSec));
            envelope *= envelope;
            float sample = (Mathf.Sin(2f * Mathf.PI * freqA * t) * 0.65f
                            + Mathf.Sin(2f * Mathf.PI * freqB * t) * 0.35f) * volume * envelope;
            samples[i] = (short)Mathf.Clamp(sample * short.MaxValue, short.MinValue, short.MaxValue);
        }

        File.WriteAllBytes(assetPath, BuildWav(samples, sampleRate));
    }

    static void WriteSweep(string assetPath, float startFreq, float endFreq, float durationSec, float volume)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSec));
        var samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float p = t / durationSec;
            float freq = Mathf.Lerp(startFreq, endFreq, p);
            float envelope = Mathf.Clamp01(1f - p);
            float sample = Mathf.Sin(2f * Mathf.PI * freq * t) * volume * envelope;
            samples[i] = (short)Mathf.Clamp(sample * short.MaxValue, short.MinValue, short.MaxValue);
        }

        File.WriteAllBytes(assetPath, BuildWav(samples, sampleRate));
    }

    private static byte[] BuildWav(short[] samples, int sampleRate)
    {
        int channels = 1;
        int bitsPerSample = 16;
        int byteRate = sampleRate * channels * bitsPerSample / 8;
        int blockAlign = channels * bitsPerSample / 8;
        int dataSize = samples.Length * sizeof(short);

        using var stream = new MemoryStream(44 + dataSize);
        using var writer = new BinaryWriter(stream);

        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataSize);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((short)blockAlign);
        writer.Write((short)bitsPerSample);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        writer.Write(dataSize);

        for (int i = 0; i < samples.Length; i++)
            writer.Write(samples[i]);

        return stream.ToArray();
    }
}
