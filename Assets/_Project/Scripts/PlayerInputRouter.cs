using UnityEngine;

[DefaultExecutionOrder(-100)]
public class PlayerInputRouter : MonoBehaviour
{
    public float pcSensitivity = 2f;
    public float touchSensitivity = 0.15f;
    public bool lockCursorOnPC = true;

    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool Sprint { get; private set; }
    public bool Fire { get; private set; }
    public bool AimHeld { get; private set; }
    public bool Jump { get; private set; }
    public bool Reload { get; private set; }
    public bool SwitchCamera { get; private set; }
    public bool CrouchPressed { get; private set; }
    public bool PronePressed { get; private set; }

    bool cursorLocked;

    void Start()
    {
        SetCursor(lockCursorOnPC && !Application.isMobilePlatform);
    }

    void SetCursor(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void Update()
    {
        bool mobile = Application.isMobilePlatform;
        bool relocked = false;

        if (!mobile && lockCursorOnPC)
        {
            if (Input.GetKeyDown(KeyCode.Escape)) SetCursor(false);
            else if (!cursorLocked && Input.GetMouseButtonDown(0))
            {
                SetCursor(true);
                relocked = true;
            }
        }

        Vector2 kb = mobile ? Vector2.zero
            : new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Move = Vector2.ClampMagnitude(kb + MobileInput.Move, 1f);

        Sprint = (!mobile && Input.GetKey(KeyCode.LeftShift))
                 || MobileInput.Move.magnitude > 0.95f;

        Vector2 look = MobileInput.Look * touchSensitivity;
        if (cursorLocked)
            look += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * pcSensitivity;
        Look = look;

        Fire = MobileInput.Fire || (cursorLocked && !relocked && Input.GetMouseButton(0));
        AimHeld = MobileInput.Aim || (cursorLocked && Input.GetMouseButton(1));
        Jump = MobileInput.JumpPressed || (!mobile && Input.GetButtonDown("Jump"));
        Reload = MobileInput.ReloadPressed || (!mobile && Input.GetKeyDown(KeyCode.R));
        SwitchCamera = MobileInput.CameraPressed || (!mobile && Input.GetKeyDown(KeyCode.V));
        CrouchPressed = MobileInput.CrouchPressed
            || (!mobile && (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.LeftControl)));
        PronePressed = MobileInput.PronePressed || (!mobile && Input.GetKeyDown(KeyCode.Z));
    }

    void LateUpdate()
    {
        MobileInput.ClearOneShots();
    }
}