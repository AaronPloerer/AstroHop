using UnityEngine.EventSystems;
using UnityEngine;

public class VolumeButtonControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isHeld;
    [SerializeField] private float volumeChangeSpeed; 
    private float volume;

    public void OnPointerDown(PointerEventData eventData)
    {
        volume = PlayerPrefs.GetFloat("MusicVolume", 0.3f);
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        isHeld = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHeld = false;
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        PlayerPrefs.Save();
    }

    void Update()
    {
        if (isHeld)
        {
            volume += volumeChangeSpeed * Time.deltaTime;

            volume = Mathf.Clamp(volume, 0f, 0.6f);

            PlayerPrefs.SetFloat("MusicVolume", volume);
        }
    }
}