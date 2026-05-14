using UnityEngine;

public class JukeboxInteractable : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;

    [Header("State (Read Only)")]
    [SerializeField] private bool isPlaying = true;

    private void Start()
    {
        // Sync the starting state with the AudioSource
        if (musicSource != null)
        {
            if (isPlaying)
            {
                if (!musicSource.isPlaying)
                    musicSource.Play();
            }
            else
            {
                musicSource.Pause();
            }
        }
    }

    // Called by the LookInteractor when the player interacts with the jukebox.
    // Rename this method if your interaction system expects a different name.
    public void Interact()
    {
        ToggleMusic();
    }

    public void ToggleMusic()
    {
        if (musicSource == null)
        {
            Debug.LogWarning("[Jukebox] No AudioSource assigned.");
            return;
        }

        if (isPlaying)
        {
            // Pause keeps the playback position — no reset
            musicSource.Pause();
            isPlaying = false;
            Debug.Log("[Jukebox] Music paused.");
        }
        else
        {
            // UnPause resumes from the exact same position
            musicSource.UnPause();
            isPlaying = true;
            Debug.Log("[Jukebox] Music resumed.");
        }
    }
}