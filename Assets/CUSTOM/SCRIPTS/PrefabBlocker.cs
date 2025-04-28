using UnityEngine;
using UnityEngine.EventSystems;

public class PrefabBlocker : MonoBehaviour, IPointerDownHandler
{
    // this fires on the very first press of the button, before the onClick
    public void OnPointerDown(PointerEventData eventData)
    {
        var blocker = FindFirstObjectByType<BlockUserView>();
        if (blocker != null)
            blocker.ShowBlocker();
    }
}
