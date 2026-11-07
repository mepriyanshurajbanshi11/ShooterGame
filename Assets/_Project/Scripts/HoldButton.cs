using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum Kind { Fire, Aim, Jump, Reload, Camera, Crouch, Prone }
    public Kind kind;

    public void OnPointerDown(PointerEventData e)
    {
        switch (kind)
        {
            case Kind.Fire: MobileInput.Fire = true; break;
            case Kind.Aim: MobileInput.Aim = true; break;
            case Kind.Jump: MobileInput.JumpPressed = true; break;
            case Kind.Reload: MobileInput.ReloadPressed = true; break;
            case Kind.Camera: MobileInput.CameraPressed = true; break;
            case Kind.Crouch: MobileInput.CrouchPressed = true; break;
            case Kind.Prone: MobileInput.PronePressed = true; break;
        }
    }

    public void OnPointerUp(PointerEventData e)
    {
        if (kind == Kind.Fire) MobileInput.Fire = false;
        if (kind == Kind.Aim) MobileInput.Aim = false;
    }
}