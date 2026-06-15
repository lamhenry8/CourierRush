using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;
    public DeliveryManager deliveryManager;

    void Start()
    {
        musicSource.loop = true;
        musicSource.Play();

        if (deliveryManager != null)
            deliveryManager.OnRestart += HandleRestart;
    }

    void OnDestroy()
    {
        if (deliveryManager != null)
            deliveryManager.OnRestart -= HandleRestart;
    }

    private void HandleRestart()
    {
        musicSource.Stop();
        musicSource.Play();
    }
}
