using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayOnStart : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip clip;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (clip != null)
            audioSource.PlayOneShot(clip);
        else
            Debug.LogWarning("[PlayOnStart] Nessun clip assegnato!");
    }
}