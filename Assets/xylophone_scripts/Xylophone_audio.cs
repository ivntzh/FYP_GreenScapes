using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class XylophoneAudio : MonoBehaviourPun
{
    [Tooltip("Assign your clang sound here")]
    public AudioClip noteClip;

    AudioSource _audio;

    void Awake()
    {
        // Add an AudioSource so we can PlayOneShot in 3D space
        _audio = gameObject.AddComponent<AudioSource>();
        _audio.spatialBlend = 1f;    // full 3D
        _audio.playOnAwake  = false;
        _audio.clip         = noteClip;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("baqueta")) return;

        // Broadcast the note to everyone (including ourselves)
        photonView.RPC(nameof(PlayNoteRPC), RpcTarget.All);
    }

    [PunRPC]
    void PlayNoteRPC()
    {
        // Fire-and-forget; every client runs this
        _audio.PlayOneShot(noteClip);
    }
}
