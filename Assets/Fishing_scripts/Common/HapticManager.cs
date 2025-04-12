using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance;

    [Header("Haptic Impulse Players")]
    public HapticImpulsePlayer leftHapticPlayer;
    public HapticImpulsePlayer rightHapticPlayer;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Sends a haptic pulse to the left controller.
    /// </summary>
    public void PulseLeft(float amplitude = 0.7f, float duration = 0.2f)
    {
        if (leftHapticPlayer != null)
            leftHapticPlayer.SendHapticImpulse(amplitude, duration);
    }

    /// <summary>
    /// Sends a haptic pulse to the right controller.
    /// </summary>
    public void PulseRight(float amplitude = 0.7f, float duration = 0.2f)
    {
        if (rightHapticPlayer != null)
            rightHapticPlayer.SendHapticImpulse(amplitude, duration);
    }

    /// <summary>
    /// Sends haptics to both controllers.
    /// </summary>
    public void PulseBoth(float amplitude = 0.7f, float duration = 0.2f)
    {
        PulseLeft(amplitude, duration);
        PulseRight(amplitude, duration);
    }
}
