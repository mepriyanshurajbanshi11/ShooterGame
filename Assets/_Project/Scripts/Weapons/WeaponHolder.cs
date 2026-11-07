using UnityEngine;

public class WeaponHolder : MonoBehaviour
{
    public WeaponData[] slots = new WeaponData[3];
    public Gun gun;
    public CameraSwitch cameraSwitch;

    PlayerInputRouter input;
    int currentSlot;

    public int CurrentSlot => currentSlot;
    public WeaponData CurrentWeapon => (slots != null && currentSlot >= 0 && currentSlot < slots.Length) ? slots[currentSlot] : null;

    void Awake()
    {
        // Cache local components early so references are guaranteed valid across lifecycle events
        if (gun == null) gun = GetComponent<Gun>();
        if (cameraSwitch == null) cameraSwitch = GetComponent<CameraSwitch>();
        input = GetComponent<PlayerInputRouter>();

        if (gun == null) Debug.LogError("WeaponHolder: Gun component is missing on " + gameObject.name);
        if (cameraSwitch == null) Debug.LogError("WeaponHolder: CameraSwitch component is missing on " + gameObject.name);
        if (input == null) Debug.LogError("WeaponHolder: PlayerInputRouter component is missing on " + gameObject.name);
    }

    void Start()
    {
        // Equip first slot as default weapon at match start
        if (slots != null && slots.Length > 0 && slots[0] != null)
        {
            SwitchToSlot(0);
        }
        else
        {
            Debug.LogError("WeaponHolder: Slot 0 has no WeaponData assigned on " + gameObject.name);
        }
    }

    void Update()
    {
        if (input == null) return;

        // PlayerInputRouter provides requested slot index (0, 1, 2) or -1 if no switch input occurred
        int requestedSlot = input.SwitchWeaponSlot;
        if (requestedSlot >= 0 && requestedSlot < slots.Length && requestedSlot != currentSlot)
        {
            SwitchToSlot(requestedSlot);
        }
    }

    public void SwitchToSlot(int slotIndex)
    {
        if (slots == null || slotIndex < 0 || slotIndex >= slots.Length)
        {
            Debug.LogError($"WeaponHolder: Slot index {slotIndex} is out of bounds on " + gameObject.name);
            return;
        }

        WeaponData data = slots[slotIndex];
        if (data == null)
        {
            Debug.LogError($"WeaponHolder: Slot {slotIndex} has no WeaponData assigned on " + gameObject.name);
            return;
        }

        currentSlot = slotIndex;

        // Keep input router active index in sync for relative scroll wheel cycling
        if (input != null)
        {
            input.SetActiveWeaponSlot(currentSlot);
        }

        if (gun != null)
        {
            gun.SetWeapon(data);
        }

        if (cameraSwitch != null)
        {
            // Update camera aiming zoom steps to match weapon's optics
            if (data.allowedScopeFovs != null && data.allowedScopeFovs.Length > 0)
            {
                cameraSwitch.scopeFovs = data.allowedScopeFovs;
                if (cameraSwitch.scopeIndex >= cameraSwitch.scopeFovs.Length)
                {
                    cameraSwitch.scopeIndex = 0;
                }
            }
        }
    }
}
