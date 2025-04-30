using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(AudioSource))]
public class PlayEasterSound : MonoBehaviour
{
    [Tooltip("Sound to play when a valid object enters this zone")]
    public AudioClip clip;

    [Tooltip("Which layers are allowed to trigger the sound")]
    public LayerMask triggerLayers = ~0; // default: everything

    [Range(0f,1f)]
    [Tooltip("Playback volume (0–1)")]
    public float volume = 1f;

    private AudioSource _audio;

    void Awake()
    {
        // Set up our AudioSource
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake  = false;
        _audio.spatialBlend = 1f;    // full 3D
        _audio.clip         = clip;
        _audio.volume       = volume;

        // Ensure our BoxCollider is a trigger
        var bc = GetComponent<BoxCollider>();
        bc.isTrigger = true;
    }

    private void OnValidate()
    {
        // Keep volume in sync in editor
        if (_audio != null)
            _audio.volume = volume;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1) Filter by layer
        if (((1 << other.gameObject.layer) & triggerLayers) == 0)
            return;

        // 2) Don’t retrigger if already playing
        if (_audio.isPlaying)
            return;

        // 3) Play locally
        _audio.PlayOneShot(clip, volume);
    }
}
