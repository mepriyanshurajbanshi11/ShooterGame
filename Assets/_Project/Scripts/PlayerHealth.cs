using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float respawnDelay = 3f;

    public float Health { get; private set; }
    public bool IsDead { get; private set; }

    CharacterController cc;
    PlayerController pc;
    Gun gun;
    Vector3 startPos;
    Quaternion startRot;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        pc = GetComponent<PlayerController>();
        gun = GetComponent<Gun>();
        startPos = transform.position;
        startRot = transform.rotation;
        Health = maxHealth;
    }

    void Update()
    {
        // TEST ONLY: press K to hurt yourself. Remove later.
        if (Input.GetKeyDown(KeyCode.K)) TakeDamage(25f);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        Health = Mathf.Max(0f, Health - amount);
        if (Health <= 0f) Die();
    }

    void Die()
    {
        IsDead = true;
        pc.enabled = false;
        gun.enabled = false;
        Invoke(nameof(Respawn), respawnDelay);
    }

    void Respawn()
    {
        cc.enabled = false;               // must be off to teleport
        transform.SetPositionAndRotation(startPos, startRot);
        cc.enabled = true;

        Health = maxHealth;
        IsDead = false;
        pc.enabled = true;
        gun.enabled = true;
        gun.Refill();
    }
}