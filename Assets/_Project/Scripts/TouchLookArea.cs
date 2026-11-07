using UnityEngine;
using UnityEngine.EventSystems;

public class TouchLookArea : MonoBehaviour, IDragHandler
{
    public void OnDrag(PointerEventData e)
    {
        MobileInput.Look += e.delta;
    }
}