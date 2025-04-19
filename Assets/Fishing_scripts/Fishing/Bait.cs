using UnityEngine;

public class Bait : MonoBehaviour
{
    private bool isHooked;
    private Transform caughtObject;

    public void HookObject(Transform target)
    {
        if (isHooked) return;

        isHooked = true;
        caughtObject = target;

        // Attach caught object to the bait
        target.position = transform.position;
        target.SetParent(transform);

        Debug.Log($"Hooked: {target.name}");
    }

    public Transform GetCaughtObject()
    {
        return caughtObject;
    }

    public void ReleaseObject()
    {
        if (caughtObject != null)
        {
            caughtObject.SetParent(null);
            caughtObject = null;
        }
        isHooked = false;
    }
}
