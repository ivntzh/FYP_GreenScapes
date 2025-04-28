using UnityEngine;

public class TutorialWateringCan : MonoBehaviour
{
    public ParticleSystem waterStream;      // Water visual effect
    public AudioSource pourSound;           // Pouring sound
    public float pourThreshold = 60f;       // Tilt angle for pouring
    public GameObject waterDetector;        // GameObject to enable/disable

    private bool isPouring = false;

    void Update()
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

    void StartPouring()
    {
        if (waterStream != null) waterStream.Play();
        if (pourSound != null && !pourSound.isPlaying) pourSound.Play();
        if (waterDetector != null) waterDetector.SetActive(true);

        isPouring = true;
        Debug.Log("💧 Pouring started");
    }

    void StopPouring()
    {
        if (waterStream != null) waterStream.Stop();
        if (pourSound != null && pourSound.isPlaying) pourSound.Stop();
        if (waterDetector != null) waterDetector.SetActive(false);

        isPouring = false;
        Debug.Log("🚫 Pouring stopped");
    }
}
