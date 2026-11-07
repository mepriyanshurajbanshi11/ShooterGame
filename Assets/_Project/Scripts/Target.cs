using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 100f;
    Renderer rend;
    Color baseColor;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        baseColor = rend.material.color;
    }

    public void Hit(float dmg, float mult)
    {
        float total = dmg * mult;
        health -= total;
        Debug.Log((mult > 1f ? "HEADSHOT " : "Body hit ") + total + " damage, health left: " + health);
        rend.material.color = Color.red;
        Invoke(nameof(ResetColor), 0.1f);
        if (health <= 0) Destroy(gameObject);
    }

    void ResetColor()
    {
        if (rend != null) rend.material.color = baseColor;
    }
}