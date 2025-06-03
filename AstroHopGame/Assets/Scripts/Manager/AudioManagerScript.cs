using UnityEngine;

public class AudioManagerScript : MonoBehaviour
{
    #region Singleton
    public static AudioManagerScript instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    #endregion

    #region Audio Sources
    public AudioSource musicSource;                         // For background music
    public AudioSource sfxSource;                           // For single SFX
    [SerializeField] private AudioSource sfxBoostSource;    // For boost SFX-Loop
    [SerializeField] private AudioSource sfxUfoSource;      // For ufo SFX-loop
    #endregion

    #region Audio Clips
    public AudioClip background;
    public AudioClip click;
    public AudioClip closeClick;
    public AudioClip jump;
    public AudioClip jump2;
    public AudioClip jump3;
    public AudioClip jump4;
    public AudioClip laserShot;
    public AudioClip explosion;
    public AudioClip breaking;
    public AudioClip fall;
    public AudioClip lowFuelBoost;
    public AudioClip highFuelBoost;
    public AudioClip ufoSound;
    public AudioClip collectFuel;
    public AudioClip collectFuelToFull;
    public AudioClip collectFuelIsFull;
    public AudioClip failedBoost;
    #endregion

    #region Boost SFX Settings
    [SerializeField] private float sfxBoostTimer;        // Minimum time before stopping boost SFX
    private float sfxBoostTime;                          // Timer tracking boost SFX duration
    private bool sfxBoostOn;                             // Boost SFX state flag
    #endregion

    #region Volume Settings
    [SerializeField] private bool backgroundMusic;       // Background music toggle
    public float defaultMusicVolume;                     // default music volume level
    public float defaultSfxVolume;                       // default sfx volume level
    [SerializeField] private float lowBoostVolume;       // Low fuel boost sound (loop) volume level
    [SerializeField] private float highBoostVolume;      // High fuel boost sound (loop) volume level
    [SerializeField] private float ufoVolume;            // UFO sound (loop) volume multiplier

    // Public volume levels for various SFX
    public float fallVolume;
    public float breakingVolume;
    public float explosionVolume;
    public float laserShotVolume;
    public float jumpVolume;
    public float jump2Volume;
    public float jump3Volume;
    public float jump4Volume;
    public float closeClickVolume;
    public float clickVolume;
    public float collectFuelVolume;
    public float collectFuelToFullVolume;
    public float collectFuelIsFullVolume;
    public float failedBoostVolume;
    #endregion

    #region Initialization
    void Start()
    {
        // Initialize volumes from saved values
        musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", AudioManagerScript.instance.defaultMusicVolume);
        sfxSource.volume = PlayerPrefs.GetFloat("SfxVolume", AudioManagerScript.instance.defaultMusicVolume);

        // Initialize boost SFX state
        sfxBoostOn = false;
        sfxBoostTime = 0;

        // Start background music if enabled
        if (backgroundMusic)
        {
            musicSource.clip = background;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
    #endregion

    #region Volume Updates
    private void Update()
    {
        // Get volumes from permanent storage
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        float sfxVolume = PlayerPrefs.GetFloat("SfxVolume", defaultSfxVolume);

        // Apply to audio sources
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
        sfxUfoSource.volume = sfxSource.volume * ufoVolume;

        // Update sfx timer while boost is active; Boost volume is update in PlayBoostSFX()
        if (sfxBoostOn)
        {
            sfxBoostTime += Time.deltaTime;
        }
    }
    #endregion

    #region Audio Methods
    // Basic SFX playback 
    public void PlaySFX(AudioClip clip, float volume)
    {
        sfxSource.PlayOneShot(clip, volume);
    }

    // UFO loop sound control methods
    public void PlayUfoSFX()
    {
        // Start boost SFX if not already playing
        if (!sfxUfoSource.isPlaying)
        {
            sfxUfoSource.clip = ufoSound;
            sfxUfoSource.loop = true;
            sfxUfoSource.Play();
        }
    }

    public void PauseUfoSFX()
    {
        if (sfxUfoSource.isPlaying)
        {
            sfxUfoSource.Pause();
        }
    }

    public void StopUfoSFX()
    {
        if (sfxUfoSource.isPlaying)
        {
            sfxUfoSource.Stop();
        }
    }

    // Boost loop sound control methods
    public void PlayBoostSFX()
    {
        // Determine boost clip and volume based on boost state
        bool fuelIsHigh = (MainGameUIScript.instance.currentFuel > MainGameUIScript.instance.lowFuelWarningValue || PlayerControllerScript.instance.startingBoost);

        AudioClip targetClip = fuelIsHigh ? highFuelBoost : lowFuelBoost;

        if(fuelIsHigh)
        {
            sfxBoostSource.volume = sfxSource.volume * highBoostVolume;
        }
        else
        {
            sfxBoostSource.volume = sfxSource.volume * lowBoostVolume;
        }

        // Start boost SFX if not already playing
        if (sfxBoostSource.clip != targetClip || !sfxBoostSource.isPlaying)
        {
            sfxBoostSource.clip = targetClip;
            sfxBoostSource.loop = true;
            sfxBoostSource.Play();
            sfxBoostOn = true;
        }
    }

    public void PauseBoostSFX()
    {
        if (sfxBoostSource.isPlaying)
        {
            sfxBoostSource.Pause();
        }
    }

    public void StopBoostSFX()
    {
        // Only stop boost SFX after minimum duration
        if (sfxBoostSource.isPlaying && sfxBoostTime > sfxBoostTimer)
        {
            sfxBoostSource.Stop();
            sfxBoostOn = false;
            sfxBoostTime = 0;
        }
    }
    #endregion
}
