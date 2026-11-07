using UnityEngine;
using UnityEngine.UI;

public class SimpleHUD : MonoBehaviour
{
    public Gun gun;
    public PlayerHealth health;
    public Text ammoText, healthText, deathText;

    void Update()
    {
        if (gun != null && ammoText != null)
        {
            ammoText.text = gun.Ammo + " / " + gun.magSize;
            ammoText.color = gun.Ammo == 0 ? Color.red : Color.white;
        }

        if (health != null && healthText != null)
        {
            healthText.text = "HP " + Mathf.CeilToInt(health.Health);
            healthText.color = health.Health <= 30f ? Color.red : Color.white;
        }

        if (health != null && deathText != null)
            deathText.gameObject.SetActive(health.IsDead);
    }
}