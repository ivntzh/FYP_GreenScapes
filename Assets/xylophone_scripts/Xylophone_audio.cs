using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Xylophone_audio : MonoBehaviour
{
    public AudioClip Xylo_AudioClip;       // Reference to the AudioClip

    // This method is called when a collision happens
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("OnCollisionEnter triggered with " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("baqueta"))
        {
            Debug.Log("Collision with baqueta detected!");
            PlaySound(); // Play the sound effect
        }
        else
        {
            Debug.Log("Collision not detected with a baqueta object.");
        }
    }

    private void PlaySound()
    {
        if (Xylo_AudioClip == null)
        {
            Debug.LogError("Audioclip missing!");
            return; // Exit early if nothing to play.
        }
        
        // Play the clip at the current transform position.
        AudioSource.PlayClipAtPoint(Xylo_AudioClip, transform.position);
    }



}
