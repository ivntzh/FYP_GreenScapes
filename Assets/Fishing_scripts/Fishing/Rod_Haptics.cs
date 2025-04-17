using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Rod_Haptics : MonoBehaviour
{
    public XRNode hand = XRNode.RightHand;
    private InputDevice device;

    void Start()
    {
        // Populate our device list for the chosen hand
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(hand, devices);

        if (devices.Count > 0)
            device = devices[0];
        else
            Debug.LogWarning($"No device found for {hand}");
    }

    /// <summary>
    /// Fire a haptic impulse if supported.
    /// </summary>
    public void Pulse(float amplitude = 0.5f, float duration = 0.2f)
    {
        if (!device.isValid)
            return;

        // Check capabilities
        if (device.TryGetHapticCapabilities(out HapticCapabilities caps) && caps.supportsImpulse)
        {
            // channel 0 is almost always fine
            device.SendHapticImpulse(0, amplitude, duration);  // :contentReference[oaicite:0]{index=0}
        }
    }
}
