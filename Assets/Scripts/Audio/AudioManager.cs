using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Central audio manager — handles all game sounds.
/// Add to every scene via Tools → Setup Audio.
///
/// Sounds:
///   Background   — looping music during MainGameL1
///   Button       — click SFX on any button
///   Countdown    — single countdown clip at level start
///   Victory      — plays on VictoryScene load
///   GameOver     — piano plays on DeathScene load
///   Narrative    — plays during intro narration in StartScene
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // ── Clips (assign in Inspector or via Setup Audio editor tool) ────────────
    [Header("Audio Clips")]
    public AudioClip backgroundMusic;   // L1 — Morning_on_the_Plateau.mp3
    [Tooltip("Start menu and main-menu bed. Sunlight in the Great Hall.")]
    public AudioClip startMenuMusic;
    public AudioClip level2Music;       // L2 — Sunlight_in_the_Great_Hall.mp3
    public AudioClip level3Music;       // L3 — Feet_Against_the_Earth.mp3
    public AudioClip buttonSound;       // button_sound.mp3
    public AudioClip countdownSound;    // count_down_sound.mp3
    public AudioClip victorySound;      // game_victory_sound.mp3
    public AudioClip gameOverSound;     // piano_GameOver_Sound.mp3
    public AudioClip narrativeSound;    // narrative_mp3.mp3
    [Tooltip("Optional second loop for richer in-game music (defaults to background clip).")]
    public AudioClip backgroundMusicLayer;

    [Header("Pickup SFX")]
    public AudioClip artifactPickupSound;
    public AudioClip healthPickupSound;
    public AudioClip sneakPickupSound;
    public AudioClip speedPickupSound;
    public AudioClip jumpPickupSound;

    [Header("Obstacle SFX")]
    public AudioClip obstacleHitSound;
    public AudioClip slowDownObstacleSound;

    [Header("Cutscene SFX (optional — procedural fallback if blank)")]
    public AudioClip curtainCutsceneSound;
    public AudioClip securityDoorSound;
    public AudioClip discoBallSound;

    // ── Audio sources ─────────────────────────────────────────────────────────
    [Header("Volume")]
    [Range(0f, 1f)] public float musicVolume    = 0.5f;
    [Range(0f, 1f)] public float sfxVolume      = 0.8f;
    [Range(0f, 1f)] public float narrativeVolume = 1.0f;

    private AudioSource _musicSource;
    private AudioSource _musicLayerSource;
    private AudioSource _sfxSource;
    private AudioSource _narrativeSource;
    private AudioSource _countdownSource;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        var existingSources = GetComponents<AudioSource>();
        for (int i = 0; i < existingSources.Length; i++)
        {
            if (existingSources[i] == null) continue;
            existingSources[i].playOnAwake = false;
        }

        // A copy ships in every scene. Only the first one may play, otherwise
        // the Opera clip on those copies keeps sounding after the scene changes.
        if (Instance != null && Instance != this)
        {
            enabled = false;
            StopSources(existingSources);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicSource      = existingSources.Length > 0 ? existingSources[0] : gameObject.AddComponent<AudioSource>();
        _musicLayerSource = gameObject.AddComponent<AudioSource>();
        _sfxSource        = gameObject.AddComponent<AudioSource>();
        _narrativeSource  = gameObject.AddComponent<AudioSource>();

        _musicSource.loop        = true;
        _musicSource.volume      = musicVolume;
        _musicSource.playOnAwake = false;
        _musicSource.spatialBlend = 0f;

        _musicLayerSource.loop        = true;
        _musicLayerSource.volume      = musicVolume * 0.35f;
        _musicLayerSource.playOnAwake = false;
        _musicLayerSource.pitch       = 1.02f;
        _musicLayerSource.spatialBlend = 0f;

        _sfxSource.loop        = false;
        _sfxSource.volume      = sfxVolume;
        _sfxSource.playOnAwake = false;

        _narrativeSource.loop        = false;
        _narrativeSource.volume      = narrativeVolume;
        _narrativeSource.playOnAwake = false;

        _countdownSource = gameObject.AddComponent<AudioSource>();
        _countdownSource.loop         = false;
        _countdownSource.volume       = sfxVolume;
        _countdownSource.playOnAwake  = false;
        _countdownSource.spatialBlend = 0f;
        _countdownSource.priority     = 64;

        if (GetComponent<MusicBeatClock>() == null)
            gameObject.AddComponent<MusicBeatClock>();

        AutoPlayForScene(SceneManager.GetActiveScene().name);
    }

    static void StopSources(AudioSource[] sources)
    {
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] != null && sources[i].isPlaying)
                sources[i].Stop();
        }
    }

    public AudioSource MusicSource => _musicSource;

    internal void RepairSingleton()
    {
        if (Instance == null)
            Instance = this;
    }

    private void OnEnable()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        if (Instance != this) return;
        AutoPlayForScene(scene.name);
    }

    // ── Auto-play logic per scene ─────────────────────────────────────────────
    private void AutoPlayForScene(string sceneName)
    {
        if (_musicSource == null) return;

        _musicSource.Stop();
        if (_musicLayerSource != null)
            _musicLayerSource.Stop();
        if (_narrativeSource != null)
            _narrativeSource.Stop();
        // Drops the victory one-shot of the Opera track, which otherwise
        // keeps playing on the SFX source after the scene changes.
        if (_sfxSource != null)
            _sfxSource.Stop();

        SilenceOperaClipOutsideOpera(sceneName);

        switch (sceneName)
        {
            case "StartScene":
                PlayBackground(startMenuMusic, useMusicLayer: false);
                break;

            case "MainGameL1Opera":
            case "MainGameL1":
                PlayBackground(backgroundMusic, useMusicLayer: false);
                break;

            case "Level2Museum":
            case "MainGameL2":
                PlayBackground(level2Music, useMusicLayer: false);
                break;

            case "Level3UndergroundDance":
            case "MainGameL3":
                PlayBackground(level3Music, useMusicLayer: false);
                break;

            case "Level2IntroScene":
            case "Level3IntroScene":
                break;

            case "VictoryScene":
                PlayVictory();
                break;

            case "DeathScene":
                PlayGameOver();
                break;
        }
    }

    static bool IsOperaScene(string sceneName)
    {
        return sceneName is "MainGameL1Opera" or "MainGameL1";
    }

    /// <summary>
    /// Morning on the Plateau is the Opera House loop only. Stop any other
    /// source that still has that clip after a scene change.
    /// </summary>
    void SilenceOperaClipOutsideOpera(string sceneName)
    {
        if (IsOperaScene(sceneName) || backgroundMusic == null) return;

        var sources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < sources.Length; i++)
        {
            var src = sources[i];
            if (src == null || src.clip != backgroundMusic) continue;
            if (src == _musicSource) continue;
            src.Stop();
            src.clip = null;
        }
    }

    public void PlayBackground()
    {
        var scene = SceneManager.GetActiveScene().name;
        if (!IsOperaScene(scene)) return;
        PlayBackground(backgroundMusic, useMusicLayer: false);
    }

    public void PlayBackground(AudioClip clip, bool useMusicLayer)
    {
        if (clip == null) return;
        _musicSource.clip   = clip;
        _musicSource.volume = musicVolume;
        _musicSource.loop   = true;
        _musicSource.Play();

        if (_musicLayerSource == null) return;

        if (useMusicLayer)
        {
            var layerClip = backgroundMusicLayer != null ? backgroundMusicLayer : clip;
            _musicLayerSource.clip   = layerClip;
            _musicLayerSource.volume = musicVolume * 0.35f;
            _musicLayerSource.loop   = true;
            _musicLayerSource.Play();
        }
        else
        {
            _musicLayerSource.Stop();
        }
    }

    public void PlayVictory()
    {
        if (victorySound == null) return;
        _musicSource.clip   = victorySound;
        _musicSource.volume = musicVolume;
        _musicSource.loop   = false;
        _musicSource.Play();
    }

    public void PlayGameOver()
    {
        if (gameOverSound == null) return;
        _musicSource.clip   = gameOverSound;
        _musicSource.volume = musicVolume;
        _musicSource.loop   = false;
        _musicSource.Play();
    }

    public void PlayNarrative()
    {
        if (narrativeSound == null) return;
        _narrativeSource.clip   = narrativeSound;
        _narrativeSource.volume = narrativeVolume;
        _narrativeSource.loop   = false;
        _narrativeSource.Play();
    }

    public void StopNarrative()
    {
        _narrativeSource.Stop();
    }

    /// <summary>Play the countdown clip once (visual numbers sync separately).</summary>
    public void PlayCountdown()
    {
        if (countdownSound == null) return;
        _countdownSource.Stop();
        _countdownSource.clip   = countdownSound;
        _countdownSource.volume = sfxVolume;
        _countdownSource.pitch  = 1f;
        _countdownSource.Play();
    }

    public void StopCountdown()
    {
        _countdownSource.Stop();
    }

    /// <summary>Deprecated — use PlayCountdown() once at countdown start.</summary>
    public void PlayCountdownTick()
    {
        PlayCountdown();
    }

    /// <summary>Play button click SFX. Wire to every Button via ButtonSoundPlayer.</summary>
    public void PlayButton()
    {
        if (buttonSound == null) return;
        _sfxSource.PlayOneShot(buttonSound, sfxVolume);
    }

    /// <summary>Shared pickup palette — pitch-shifted button SFX for cohesion with sneak.</summary>
    public void PlayPickupSfx(float pitch = 1f, float volumeScale = 0.75f)
    {
        if (buttonSound == null) return;
        _sfxSource.pitch = pitch;
        _sfxSource.PlayOneShot(buttonSound, sfxVolume * volumeScale);
        _sfxSource.pitch = 1f;
    }

    public void PlaySneakPickupSfx() => PlayClipOrFallback(sneakPickupSound, 0.82f, 0.65f);
    public void PlayArtifactPickupSfx() => PlayClipOrFallback(artifactPickupSound, 1.15f, 0.85f);
    public void PlayHealthPickupSfx() => PlayClipOrFallback(healthPickupSound, 1f, 0.7f);
    public void PlaySpeedPickupSfx() => PlayClipOrFallback(speedPickupSound, 1.35f, 0.8f);
    public void PlayJumpPickupSfx() => PlayClipOrFallback(jumpPickupSound, 1.22f, 0.75f);
    public void PlayObstacleHitSfx() => PlayClipOrFallback(obstacleHitSound, 0.55f, 0.85f);
    public void PlaySlowDownObstacleSfx() => PlayClipOrFallback(slowDownObstacleSound, 0.48f, 0.75f);

    public void PlayCutsceneStinger(LevelCutsceneType type, int phase = 0)
    {
        switch (type)
        {
            case LevelCutsceneType.OperaCurtain:
                if (curtainCutsceneSound != null)
                    _sfxSource.PlayOneShot(curtainCutsceneSound, sfxVolume);
                else if (phase == 0)
                    PlayPickupSfx(0.52f, 0.45f);
                else
                    PlayPickupSfx(0.68f, 0.55f);
                break;
            case LevelCutsceneType.MuseumSecurityDoor:
                if (securityDoorSound != null)
                    _sfxSource.PlayOneShot(securityDoorSound, sfxVolume);
                else if (phase == 0)
                    PlayPickupSfx(0.42f, 0.7f);
                else
                    PlayPickupSfx(0.58f, 0.65f);
                break;
            case LevelCutsceneType.ClubDiscoBall:
                if (discoBallSound != null)
                    _sfxSource.PlayOneShot(discoBallSound, sfxVolume);
                else if (phase == 0)
                    PlayPickupSfx(1.45f, 0.5f);
                else
                    PlayPickupSfx(1.75f, 0.6f);
                break;
        }
    }

    private void PlayClipOrFallback(AudioClip clip, float fallbackPitch, float fallbackVolumeScale)
    {
        if (clip != null)
        {
            _sfxSource.PlayOneShot(clip, sfxVolume);
            return;
        }

        PlayPickupSfx(fallbackPitch, fallbackVolumeScale);
    }

    public void StopMusic()
    {
        _musicSource.Stop();
        if (_musicLayerSource != null)
            _musicLayerSource.Stop();
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = v;
        _musicSource.volume = v;
        if (_musicLayerSource != null)
            _musicLayerSource.volume = v * 0.35f;
    }

    public void SetSfxVolume(float v)
    {
        sfxVolume = v;
        if (_countdownSource != null)
            _countdownSource.volume = v;
    }
}
