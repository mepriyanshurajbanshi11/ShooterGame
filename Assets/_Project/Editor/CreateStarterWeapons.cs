#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateStarterWeapons
{
    const string weaponsDirectory = "Assets/_Project/ScriptableObjects/Weapons";

    [MenuItem("Tools/Create Starter Weapons")]
    public static void GenerateWeapons()
    {
        // Ensure destination folder exists before writing assets
        if (!AssetDatabase.IsValidFolder(weaponsDirectory))
        {
            Directory.CreateDirectory(weaponsDirectory);
            AssetDatabase.Refresh();
        }

        CreateWeaponAsset("Assault Rifle.asset", "Assault Rifle", 20f, 2f, 0.10f, 30, 2.0f, 100f, 1, 0f, new float[] { 50f, 30f, 15f });
        CreateWeaponAsset("SMG.asset", "SMG", 12f, 2f, 0.07f, 30, 1.8f, 50f, 1, 0f, new float[] { 50f, 30f });
        CreateWeaponAsset("Sniper.asset", "Sniper", 85f, 2f, 1.4f, 5, 3.0f, 250f, 1, 0f, new float[] { 15f, 8f });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Starter weapon assets successfully created in: " + weaponsDirectory);
    }

    static void CreateWeaponAsset(string fileName, string weaponName, float damage, float headshotMultiplier,
        float fireRate, int magazineSize, float reloadTime, float range, int pellets, float spread, float[] allowedScopeFovs)
    {
        string assetPath = $"{weaponsDirectory}/{fileName}";
        WeaponData weapon = AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);
        bool isNew = weapon == null;

        if (isNew)
        {
            weapon = ScriptableObject.CreateInstance<WeaponData>();
        }

        weapon.weaponName = weaponName;
        weapon.damage = damage;
        weapon.headshotMultiplier = headshotMultiplier;
        weapon.fireRate = fireRate;
        weapon.magazineSize = magazineSize;
        weapon.reloadTime = reloadTime;
        weapon.range = range;
        weapon.pellets = pellets;
        weapon.spread = spread;
        weapon.allowedScopeFovs = allowedScopeFovs;

        if (isNew)
        {
            AssetDatabase.CreateAsset(weapon, assetPath);
        }
        else
        {
            EditorUtility.SetDirty(weapon);
        }
    }
}
#endif
