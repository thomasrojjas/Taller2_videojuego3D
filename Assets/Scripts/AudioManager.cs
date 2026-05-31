using UnityEngine;

// Maneja la musica y los sonidos del juego.
// Hay que arrastrar los clips en el Inspector. Si un clip no esta puesto,
// ese sonido simplemente no suena (no pasa nada).
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Clips de audio")]
    public AudioClip bgmClip;          // musica de la partida
    public AudioClip menuClip;         // musica del menu de derrota
    public AudioClip shootClip;        // disparo
    public AudioClip jumpClip;         // salto
    public AudioClip coinClip;         // moneda
    public AudioClip zombieDeathClip;  // muerte del zombie
    public AudioClip gameOverClip;     // derrota

    // una fuente para la musica y otra para los efectos
    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        // singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // creamos las dos fuentes de audio
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true; // la musica se repite
        musicSource.playOnAwake = false;
        musicSource.volume = 0.4f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    // musica de fondo de la partida
    public void PlayBGM()
    {
        if (bgmClip != null)
        {
            musicSource.clip = bgmClip;
            musicSource.Play();
        }
    }

    // para la musica
    public void StopBGM()
    {
        musicSource.Stop();
    }

    // musica del menu de derrota
    public void PlayMenuMusic()
    {
        if (menuClip != null)
        {
            musicSource.clip = menuClip;
            musicSource.Play();
        }
    }

    // sonido del disparo
    public void PlayShootSFX()
    {
        if (shootClip != null)
        {
            sfxSource.PlayOneShot(shootClip);
        }
    }

    // sonido del salto
    public void PlayJumpSFX()
    {
        if (jumpClip != null)
        {
            sfxSource.PlayOneShot(jumpClip);
        }
    }

    // sonido de la moneda
    public void PlayCoinSFX()
    {
        if (coinClip != null)
        {
            sfxSource.PlayOneShot(coinClip);
        }
    }

    // sonido de cuando muere un zombie
    public void PlayZombieDeathSFX()
    {
        if (zombieDeathClip != null)
        {
            sfxSource.PlayOneShot(zombieDeathClip);
        }
    }

    // sonido de derrota (ademas para la musica)
    public void PlayGameOverSFX()
    {
        StopBGM();
        if (gameOverClip != null)
        {
            sfxSource.PlayOneShot(gameOverClip);
        }
    }
}
