using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class HeadHitAudioTrigger : MonoBehaviourPun
{
    [Tooltip("Sound played when head is hit by the stick")]
    public AudioClip hitClip;

    AudioSource _audio;

    void Awake()
    {
        // Add a 3D AudioSource so PlayOneShot sounds come from head position
        _audio = gameObject.AddComponent<AudioSource>();
        _audio.spatialBlend = 1f;    // full 3D
        _audio.playOnAwake  = false;
        _audio.clip         = hitClip;
    }

    private void OnTriggerEnter(Collider other)
    {
        // only respond to the stick
        if (!other.CompareTag("baqueta"))
            return;

        // tell everyone to play the hit sound
        photonView.RPC(nameof(PlayHitSoundRPC), RpcTarget.All);
    }

    [PunRPC]
    void PlayHitSoundRPC()
    {
        if (hitClip != null)
            _audio.PlayOneShot(hitClip);
    }
}
