using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public WeaponData weaponData;
    public Transform camPivot;

    PlayerInputRouter input;
    int ammo;
    float nextFire;
    bool isReloading;
    Coroutine reloadCoroutine;

    // Retained for SimpleHUD compatibility
    public int Ammo => ammo;
    public int magSize => weaponData != null ? weaponData.magazineSize : 0;
    public bool IsReloading => isReloading;

    void Awake()
    {
        // Cache input router to avoid per-frame lookups
        input = GetComponent<PlayerInputRouter>();
        if (input == null)
        {
            Debug.LogError("Gun: PlayerInputRouter missing on " + gameObject.name);
        }

        if (camPivot == null)
        {
            Debug.LogError("Gun: CamPivot reference is not assigned on " + gameObject.name);
        }
    }

    void Start()
    {
        if (weaponData == null)
        {
            Debug.LogError("Gun: WeaponData is not assigned on " + gameObject.name);
            return;
        }

        ammo = weaponData.magazineSize;
    }

    public void Refill()
    {
        if (weaponData == null)
        {
            Debug.LogError("Gun: Cannot refill because WeaponData is null on " + gameObject.name);
            return;
        }

        // Cancel in-flight reloads so the magazine fills immediately
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
            isReloading = false;
        }

        ammo = weaponData.magazineSize;
    }

    public void SetWeapon(WeaponData data)
    {
        if (data == null)
        {
            Debug.LogError("Gun: Cannot set null WeaponData on " + gameObject.name);
            return;
        }

        // Interrupt active reload routine when swapping weapons so the previous timer does not refill the new gun
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
            isReloading = false;
        }

        weaponData = data;
        ammo = weaponData.magazineSize;
    }

    void Update()
    {
        if (weaponData == null || input == null) return;

        // Player explicitly requested reload and there is room in the magazine
        if (input.Reload && !isReloading && ammo < weaponData.magazineSize)
        {
            reloadCoroutine = StartCoroutine(ReloadRoutine());
        }

        // Prevent firing while reloading, empty, or before next allowed fire interval
        if (!isReloading && input.Fire && Time.time >= nextFire && ammo > 0)
        {
            nextFire = Time.time + weaponData.fireRate;
            ammo--;

            FirePellets();
        }
    }

    void FirePellets()
    {
        if (camPivot == null) return;

        int pelletCount = Mathf.Max(1, weaponData.pellets);
        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 fireDir = camPivot.forward;

            // Distribute raycasts within cone of spread specified in degrees
            if (weaponData.spread > 0f)
            {
                Vector2 circle = Random.insideUnitCircle * weaponData.spread;
                Quaternion spreadRot = Quaternion.Euler(circle.y, circle.x, 0f);
                fireDir = camPivot.rotation * spreadRot * Vector3.forward;
            }

            if (Physics.Raycast(camPivot.position, fireDir, out RaycastHit hit, weaponData.range))
            {
                // Discard hits on player's own colliders
                if (hit.collider.transform.IsChildOf(transform)) continue;

                var target = hit.collider.GetComponentInParent<IDamageable>();
                if (target != null)
                {
                    bool head = hit.point.y > hit.collider.bounds.max.y - 0.3f;
                    float total = weaponData.damage * (head ? weaponData.headshotMultiplier : 1f);
                    if (head) Debug.Log("HEADSHOT");
                    target.TakeDamage(total);
                }
            }
        }
    }

    IEnumerator ReloadRoutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(weaponData.reloadTime);

        if (weaponData != null)
        {
            ammo = weaponData.magazineSize;
        }

        isReloading = false;
        reloadCoroutine = null;
    }
}
