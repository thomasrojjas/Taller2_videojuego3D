using UnityEngine;

/// <summary>
/// Singleton audio manager. Manages background music and game sound effects.
/// FALLBACK: If no audio clips are provided (as they are not in the raw ZIP), 
/// this script implements a procedural chiptune synthesizer.
/// To avoid 'multiple AudioSource conflict warnings' with OnAudioFilterRead, 
/// the synthesizer is isolated in a separate child BGMPlayer GameObject!
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips (Optional Inspector Assignment)")]
    public AudioClip bgmClip;
    public AudioClip shootClip;
    public AudioClip jumpClip;
    public AudioClip coinClip;
    public AudioClip zombieDeathClip;
    public AudioClip gameOverClip;

    [Header("BGM Synthesizer Settings")]
    [Tooltip("Enable procedural chiptune BGM loop if no bgmClip is assigned.")]
    public bool enableProceduralBGM = true;

    private AudioSource sfxSource;
    private GameObject bgmChild;
    private AudioSource bgmSource;
    private ProceduralBGMPlayer bgmPlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
    }

    private void InitializeAudioSources()
    {
        // Setup SFX source directly on this GameObject
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f; // 2D Sound

        // Setup BGM Player on a dedicated child GameObject to avoid AudioSource filter conflicts
        bgmChild = new GameObject("BGMPlayer");
        bgmChild.transform.SetParent(this.transform);
        
        bgmSource = bgmChild.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = 0.2f; // Soft volume for chiptune BGM
        bgmSource.spatialBlend = 0f;

        bgmPlayer = bgmChild.AddComponent<ProceduralBGMPlayer>();
        bgmPlayer.enabled = false; // Enabled only when synthesising
    }

    /// <summary>
    /// Starts background music. Uses clip if assigned, otherwise activates chiptune BGM synthesizer child.
    /// </summary>
    public void PlayBGM()
    {
        if (bgmClip != null)
        {
            bgmPlayer.enabled = false;
            bgmSource.clip = bgmClip;
            if (!bgmSource.isPlaying)
            {
                bgmSource.Play();
            }
        }
        else if (enableProceduralBGM)
        {
            bgmSource.clip = null;
            bgmPlayer.enabled = true;
            if (!bgmSource.isPlaying)
            {
                bgmSource.Play(); // Must be playing to trigger filter read
            }
        }
    }

    public void StopBGM()
    {
        bgmSource.Stop();
        bgmPlayer.enabled = false;
    }

    // --- Sound Effects Player Methods ---

    public void PlayShootSFX()
    {
        if (shootClip != null)
        {
            sfxSource.PlayOneShot(shootClip);
        }
        else
        {
            PlayProceduralSFX(0); // 0 = Shoot
        }
    }

    public void PlayJumpSFX()
    {
        if (jumpClip != null)
        {
            sfxSource.PlayOneShot(jumpClip);
        }
        else
        {
            PlayProceduralSFX(1); // 1 = Jump
        }
    }

    public void PlayCoinSFX()
    {
        if (coinClip != null)
        {
            sfxSource.PlayOneShot(coinClip);
        }
        else
        {
            PlayProceduralSFX(2); // 2 = Coin
        }
    }

    public void PlayZombieDeathSFX()
    {
        if (zombieDeathClip != null)
        {
            sfxSource.PlayOneShot(zombieDeathClip);
        }
        else
        {
            PlayProceduralSFX(3); // 3 = ZombieDeath
        }
    }

    public void PlayGameOverSFX()
    {
        StopBGM();
        if (gameOverClip != null)
        {
            sfxSource.PlayOneShot(gameOverClip);
        }
        else
        {
            PlayProceduralSFX(4); // 4 = GameOver
        }
    }

    // --- Procedural Synthesis Engine ---

    private void PlayProceduralSFX(int type)
    {
        // Create an ephemeral GameObject to synthesize the sound and self-destruct when completed
        GameObject synthObj = new GameObject("ProceduralSFX");
        AudioSource source = synthObj.AddComponent<AudioSource>();
        ProceduralSoundSynth synth = synthObj.AddComponent<ProceduralSoundSynth>();
        
        synth.Initialize(type, source);
    }
}

/// <summary>
/// Dedicated BGM player component. Uses OnAudioFilterRead on its own GameObject
/// to synthesize BGM without interfering with main AudioManager's SFX sources.
/// </summary>
public class ProceduralBGMPlayer : MonoBehaviour
{
    private float bgmPhase = 0f;
    private float bgmTime = 0f;
    private int currentNoteIndex = 0;
    private float nextNoteTime = 0f;
    private float currentBgmFreq = 0f;

    // Chiptune melody notes (in Hz)
    private readonly float[] melodyNotes = new float[] 
    {
        261.63f, 293.66f, 329.63f, 349.23f, 392.00f, 349.23f, 329.63f, 293.66f, // C4 D4 E4 F4 G4 F4 E4 D4
        329.63f, 349.23f, 392.00f, 440.00f, 493.88f, 440.00f, 392.00f, 349.23f, // E4 F4 G4 A4 B4 A4 G4 F4
        392.00f, 440.00f, 493.88f, 523.25f, 587.33f, 523.25f, 493.88f, 440.00f, // G4 A4 B4 C5 D5 C5 B4 A4
        493.88f, 392.00f, 440.00f, 349.23f, 329.63f, 293.66f, 261.63f, 196.00f  // B4 G4 A4 F4 E4 D4 C4 G3
    };

    private void OnEnable()
    {
        bgmTime = 0f;
        bgmPhase = 0f;
        currentNoteIndex = 0;
        nextNoteTime = 0f;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        double samplingFrequency = AudioSettings.outputSampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            // Advance background clock
            bgmTime += 1f / (float)samplingFrequency;

            // Sequencer: change note every 0.18 seconds
            if (bgmTime >= nextNoteTime)
            {
                currentBgmFreq = melodyNotes[currentNoteIndex];
                currentNoteIndex = (currentNoteIndex + 1) % melodyNotes.Length;
                nextNoteTime = bgmTime + 0.18f;
            }

            // Generate clean chiptune sound wave (Triangle/Square combo)
            bgmPhase += (2f * Mathf.PI * currentBgmFreq) / (float)samplingFrequency;
            if (bgmPhase > 2f * Mathf.PI)
            {
                bgmPhase -= 2f * Mathf.PI;
            }

            // Triangle wave value between -1 and 1
            float triValue = 1f - 2f * Mathf.Abs(1f - (bgmPhase / Mathf.PI));
            
            // Pulse (square) wave value
            float pulseValue = Mathf.Sin(bgmPhase) > 0f ? 0.3f : -0.3f;

            // Mix and scale volume
            float sample = (triValue * 0.4f + pulseValue * 0.6f) * 0.08f;

            // Assign to all channels
            for (int c = 0; c < channels; c++)
            {
                data[i + c] = sample;
            }
        }
    }
}

/// <summary>
/// Helper script that handles real-time synthesis of sound effects.
/// Attached to procedural sound game objects.
/// </summary>
public class ProceduralSoundSynth : MonoBehaviour
{
    private int sfxType;
    private AudioSource source;
    private float elapsed = 0f;
    private float duration = 0.5f;
    private float phase = 0f;

    public void Initialize(int type, AudioSource src)
    {
        sfxType = type;
        source = src;
        
        // Define durations depending on sound type
        switch (type)
        {
            case 0: // Shoot
                duration = 0.15f;
                break;
            case 1: // Jump
                duration = 0.2f;
                break;
            case 2: // Coin
                duration = 0.25f;
                break;
            case 3: // ZombieDeath
                duration = 0.4f;
                break;
            case 4: // GameOver
                duration = 1.2f;
                break;
        }

        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D
        source.Play(); // Active source to trigger filter read

        // Self-destruct after duration + margin
        Destroy(gameObject, duration + 0.2f);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        double samplingFrequency = AudioSettings.outputSampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            elapsed += 1f / (float)samplingFrequency;
            if (elapsed > duration)
            {
                // Silence remaining buffers
                for (int c = 0; c < channels; c++)
                {
                    data[i + c] = 0f;
                }
                continue;
            }

            float sample = 0f;
            float percent = elapsed / duration;

            switch (sfxType)
            {
                case 0: // Shoot: Quick high-to-low sine frequency sweep
                    {
                        float currentFreq = Mathf.Lerp(1200f, 150f, percent);
                        phase += (2f * Mathf.PI * currentFreq) / (float)samplingFrequency;
                        sample = Mathf.Sin(phase) * (1f - percent) * 0.15f;
                    }
                    break;

                case 1: // Jump: Rapid low-to-high sine frequency sweep
                    {
                        float currentFreq = Mathf.Lerp(200f, 600f, percent);
                        phase += (2f * Mathf.PI * currentFreq) / (float)samplingFrequency;
                        sample = Mathf.Sin(phase) * (1f - percent) * 0.12f;
                    }
                    break;

                case 2: // Coin: Quick clean double-tone arpeggio (B5 -> E6)
                    {
                        float currentFreq = percent < 0.35f ? 987.77f : 1318.51f;
                        phase += (2f * Mathf.PI * currentFreq) / (float)samplingFrequency;
                        // Use square-like wave for retro coin feel
                        sample = (Mathf.Sin(phase) > 0f ? 0.08f : -0.08f) * (1f - percent);
                    }
                    break;

                case 3: // ZombieDeath: Low growling white noise / rumble
                    {
                        float rumble = Random.Range(-1f, 1f);
                        float currentFreq = Mathf.Lerp(80f, 20f, percent);
                        phase += (2f * Mathf.PI * currentFreq) / (float)samplingFrequency;
                        // Mix rumble and sine growl
                        sample = (rumble * 0.4f + Mathf.Sin(phase) * 0.6f) * (1f - percent) * 0.25f;
                    }
                    break;

                case 4: // GameOver: Sad descending chiptune arpeggio
                    {
                        float currentFreq;
                        if (percent < 0.25f) currentFreq = 392f; // G4
                        else if (percent < 0.5f) currentFreq = 349f; // F4
                        else if (percent < 0.75f) currentFreq = 311f; // Eb4
                        else currentFreq = 261f; // C4

                        phase += (2f * Mathf.PI * currentFreq) / (float)samplingFrequency;
                        // Combines triangle wave for somber tone
                        float tri = 1f - 2f * Mathf.Abs(1f - (phase % (2f * Mathf.PI) / Mathf.PI));
                        sample = tri * (1f - percent) * 0.15f;
                    }
                    break;
            }

            // Assign synthesized sample to all channels
            for (int c = 0; c < channels; c++)
            {
                data[i + c] = sample;
            }
        }
    }
}
