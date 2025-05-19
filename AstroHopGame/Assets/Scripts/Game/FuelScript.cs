using UnityEngine;

public class FuelScript : MonoBehaviour
{
    #region Fuel Properties
    [SerializeField] private float addedFuel;
    #endregion

    #region Collision Handling
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Play pick-up sound, load up rocket boost and delete itself
            PickUpSound();
            UpdateFuel();
            Destroy(gameObject);
        }
    }

    private void PickUpSound()
    {
        // Change pick-up sound if player already has full fuel capacity or if it reaches full capacity
        if (MainGameUIScript.instance.currentFuel == MainGameUIScript.instance.maxFuel)
        {
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.collectFuelIsFull, AudioManagerScript.instance.collectFuelIsFullVolume);
        }
        else if (MainGameUIScript.instance.currentFuel >= (MainGameUIScript.instance.maxFuel - addedFuel))
        {
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.collectFuelToFull, AudioManagerScript.instance.collectFuelToFullVolume);
        }
        else
        {
            AudioManagerScript.instance.PlaySFX(AudioManagerScript.instance.collectFuel, AudioManagerScript.instance.collectFuelVolume);
        }
    }

    private void UpdateFuel()
    {
        MainGameUIScript.instance.currentFuel += addedFuel;
        if (MainGameUIScript.instance.maxFuel < MainGameUIScript.instance.currentFuel)
        {
            MainGameUIScript.instance.currentFuel = MainGameUIScript.instance.maxFuel;
        }
    }
    #endregion
}