using UnityEngine;

public static class MobileInput
{
    public static Vector2 Move, Look;
    public static bool Fire, Aim;
    public static bool JumpPressed, ReloadPressed, CameraPressed;
    public static bool CrouchPressed, PronePressed;

    public static void ClearOneShots()
    {
        Look = Vector2.zero;
        JumpPressed = ReloadPressed = CameraPressed = false;
        CrouchPressed = PronePressed = false;
    }
}