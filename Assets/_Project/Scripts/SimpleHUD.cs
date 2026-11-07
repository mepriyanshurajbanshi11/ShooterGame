using UnityEngine;
using UnityEngine.UI;

public class SimpleHUD : MonoBehaviour
{
    public Gun gun;
    public Text ammoText;

    void Update()
    {
        if (gun == null || ammoText == null) return;
        ammoText.text = gun.Ammo + " / " + gun.magSize;
        ammoText.color = gun.Ammo == 0 ? Color.red : Color.white;
    }
}