using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public RectTransform background, handle;
    public float radius = 120f;

    public void OnPointerDown(PointerEventData e) { OnDrag(e); }

    public void OnDrag(PointerEventData e)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background, e.position, e.pressEventCamera, out Vector2 p);
        Vector2 v = Vector2.ClampMagnitude(p, radius);
        handle.anchoredPosition = v;
        MobileInput.Move = v / radius;
    }

    public void OnPointerUp(PointerEventData e)
    {
        handle.anchoredPosition = Vector2.zero;
        MobileInput.Move = Vector2.zero;
    }
}