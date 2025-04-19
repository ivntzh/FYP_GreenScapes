using System.Collections.Generic;
using UnityEngine;

public class xyloManager : MonoBehaviour
{
    [System.Serializable]
    public class AudioColliderPair
    {
        public Collider targetCollider; // The collider associated with the key
        public AudioClip audioClip;     // The audio clip for the key
    }

    public Dictionary<GameObject, AudioClip> keySounds = new Dictionary<GameObject, AudioClip>();

    public AudioSource audioSource; // Shared audio source

    public void PlaySoundForKey(GameObject key)
    {
        if (keySounds.ContainsKey(key))
        {
            AudioClip clip = keySounds[key];
            audioSource.PlayOneShot(clip);
        }
    }

    public List<AudioColliderPair> audioColliderPairs; // List of keys and sounds

    // Public method to play audio when a key is struck
    public void PlayAudio(Collider keyCollider)
    {
        foreach (var pair in audioColliderPairs)
        {
            if (pair.targetCollider == keyCollider)
            {
                audioSource.PlayOneShot(pair.audioClip);
                Debug.Log($"Played sound for: {keyCollider.name}");
                return;
            }
        }

        Debug.LogWarning($"No audio found for collider: {keyCollider.name}");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Play audio if the stick interacts with a key
        PlayAudio(other);
    }
}
