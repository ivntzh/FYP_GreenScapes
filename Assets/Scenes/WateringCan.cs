using UnityEngine;

public class WateringCan : MonoBehaviour
{
    public ParticleSystem waterStream;   // Water VFX
    public AudioSource pourSound;        // Pouring sound source
    public float pourThreshold = 60f;    // Z-axis tilt threshold

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

        isPouring = true;
        Debug.Log("💧 Pouring started");
    }

    void StopPouring()
    {
        if (waterStream != null) waterStream.Stop();
        if (pourSound != null && pourSound.isPlaying) pourSound.Stop();

        isPouring = false;
        Debug.Log("🚫 Pouring stopped");
    }
}
