using UnityEngine.EventSystems;
using UnityEngine;

public class VolumeButtonControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isHeld;
    private float pointerDownTime;
    private float minHoldDuration = 0.2f;
    [SerializeField] private float volumeChangeSpeed;
    private float volume;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        pointerDownTime = Time.time;
        volume = PlayerPrefs.GetFloat("MusicVolume", 0.3f);
        AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.click, AudioManagerScript.instance.clickVolume);
        isHeld = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        isHeld = false;

        float holdDuration = Time.time - pointerDownTime;
        if (holdDuration >= minHoldDuration)
        {
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.closeClick, AudioManagerScript.instance.closeClickVolume);
        }

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