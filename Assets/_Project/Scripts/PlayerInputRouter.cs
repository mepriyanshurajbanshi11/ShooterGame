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
    public int SwitchWeaponSlot { get; private set; } = -1;

    bool cursorLocked;
    int activeWeaponSlot;

    void Start()
    {
        SetCursor(lockCursorOnPC && !Application.isMobilePlatform);
    }

    public void SetActiveWeaponSlot(int slot)
    {
        activeWeaponSlot = Mathf.Clamp(slot, 0, 2);
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

        // Evaluate weapon switch requests, defaulting to -1 when no switch action is invoked this frame
        int switchSlot = -1;
        if (mobile)
        {
            if (MobileInput.SwitchWeaponSlot >= 0)
            {
                switchSlot = MobileInput.SwitchWeaponSlot;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                switchSlot = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                switchSlot = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                switchSlot = 2;
            }
            else
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll > 0.001f)
                {
                    // Scroll wheel up rotates forward through available slots
                    switchSlot = (activeWeaponSlot + 1) % 3;
                }
                else if (scroll < -0.001f)
                {
                    // Scroll wheel down rotates backward through available slots
                    switchSlot = (activeWeaponSlot - 1 + 3) % 3;
                }
            }
        }

        if (switchSlot >= 0)
        {
            activeWeaponSlot = switchSlot;
        }

        SwitchWeaponSlot = switchSlot;
    }

    void LateUpdate()
    {
        MobileInput.ClearOneShots();
    }
}
