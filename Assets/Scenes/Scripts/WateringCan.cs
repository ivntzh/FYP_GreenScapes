using UnityEngine;
using Photon.Pun;

public class WateringCan : MonoBehaviour
{
    public ParticleSystem waterStream;      // Water visual effect
    public AudioSource pourSound;           // Pouring sound
    public float pourThreshold = 60f;       // Tilt angle for pouring
    public GameObject waterDetector;        // GameObject to enable/disable
    public RecyclingManager recyclingManager; // Reference to RecyclingManager
    public PhotonView photonView;           // Reference to PhotonView

    private bool isPouring = false;

    void Update()
    {
        if (photonView.IsMine)
        {
            float tiltZ = transform.eulerAngles.z;

            if (tiltZ > 180f)
                tiltZ -= 360f;

            if (!isPouring && Mathf.Abs(tiltZ) > pourThreshold)
            {
                StartPouring();
            }
            else if (isPouring && Mathf.Abs(tiltZ) < pourThreshold - 10f)
            {
                StopPouring();
            }
        }
    }

    private void StartPouring()
    {
        if (waterStream != null) waterStream.Play();
        if (pourSound != null && !pourSound.isPlaying) pourSound.Play();
        if (waterDetector != null) waterDetector.SetActive(true);

        isPouring = true;
        Debug.Log("💧 Pouring started");

        // Notify other clients
        recyclingManager.photonView.RPC(
            nameof(RecyclingManager.RPC_StartPouring),
            RpcTarget.All
        );
    }

    [PunRPC]
    private void RPC_StartPouring()
    {
        if (waterStream != null) waterStream.Play();
        if (pourSound != null && !pourSound.isPlaying) pourSound.Play();
        if (waterDetector != null) waterDetector.SetActive(true);
        isPouring = true;
    }

    private void StopPouring()
    {
        if (waterStream != null) waterStream.Stop();
        if (pourSound != null && pourSound.isPlaying) pourSound.Stop();
        if (waterDetector != null) waterDetector.SetActive(false);

        isPouring = false;
        Debug.Log("🚫 Pouring stopped");

        // Notify other clients
        recyclingManager.photonView.RPC(
            nameof(RecyclingManager.RPC_StopPouring),
            RpcTarget.All
        );
    }

    [PunRPC]
    private void RPC_StopPouring()
    {
        if (waterStream != null) waterStream.Stop();
        if (pourSound != null && pourSound.isPlaying) pourSound.Stop();
        if (waterDetector != null) waterDetector.SetActive(false);
        isPouring = false;
    }

    public void Initialize()
    {
        // Initialize watering can
        Debug.Log("WateringCan initialized.");
    }
}