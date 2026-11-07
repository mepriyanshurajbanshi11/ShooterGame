using UnityEngine;

public class Target : MonoBehaviour, IDamageable
{
    public float health = 100f;
    Renderer rend;
    Color baseColor;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        baseColor = rend.material.color;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log("Target hit for " + amount + ", health left: " + health);
        rend.material.color = Color.red;
        Invoke(nameof(ResetColor), 0.1f);
        if (health <= 0) Destroy(gameObject);
    }

    void ResetColor()
    {
        if (rend != null) rend.material.color = baseColor;
    }
}