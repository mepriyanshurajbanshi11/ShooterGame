using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform camPivot;
    public float damage = 20f, range = 100f, fireRate = 0.1f;
    public int magSize = 30;

    PlayerInputRouter input;
    int ammo;
    float nextFire;

    public int Ammo => ammo;

    void Start()
    {
        input = GetComponent<PlayerInputRouter>();
        ammo = magSize;
    }

    void Update()
    {
        if (input.Reload) ammo = magSize;

        if (input.Fire && Time.time >= nextFire && ammo > 0)
        {
            nextFire = Time.time + fireRate;
            ammo--;

            if (Physics.Raycast(camPivot.position, camPivot.forward, out RaycastHit hit, range))
            {
                var t = hit.collider.GetComponentInParent<Target>();
                if (t != null)
                {
                    bool head = hit.point.y > hit.collider.bounds.max.y - 0.3f;
                    t.Hit(damage, head ? 2f : 1f);
                }
            }
        }
    }
}