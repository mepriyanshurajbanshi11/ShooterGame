using UnityEngine;

public class Target : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float health = 100f;
    public float respawnDelay = 2.5f;

    Renderer rend;
    Collider col;
    Color baseColor;
    bool isDead;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        if (rend != null) baseColor = rend.material.color;
        col = GetComponent<Collider>();
        health = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        health = Mathf.Max(0f, health - amount);
        Debug.Log("Target hit for " + amount + ", health left: " + health);

        if (rend != null)
        {
            rend.material.color = Color.red;
            CancelInvoke(nameof(ResetColor));
            Invoke(nameof(ResetColor), 0.12f);
        }

        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        if (rend != null) rend.enabled = false;
        if (col != null) col.enabled = false;

        Invoke(nameof(Respawn), respawnDelay);
    }

    void Respawn()
    {
        health = maxHealth;
        isDead = false;
        if (rend != null)
        {
            rend.material.color = baseColor;
            rend.enabled = true;
        }
        if (col != null) col.enabled = true;
    }

    void ResetColor()
    {
        if (rend != null && !isDead) rend.material.color = baseColor;
    }
}
