using UnityEngine;

// Holds weapon tuning and ballistic data so balancing changes do not require code edits
[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Shooter/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public float damage;
    public float headshotMultiplier = 2f;
    public float fireRate;
    public int magazineSize;
    public float reloadTime;
    public float range;
    public int pellets = 1;
    public float spread = 0f;
    public float[] allowedScopeFovs;
}
