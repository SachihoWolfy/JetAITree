using UnityEngine;

public class RandomStartAudio : MonoBehaviour
{
    public AudioSource audioSource;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource.clip != null)
        {
            audioSource.time = Random.Range(0f, audioSource.clip.length); // Set random start time
            audioSource.Play();
        }
    }
}
