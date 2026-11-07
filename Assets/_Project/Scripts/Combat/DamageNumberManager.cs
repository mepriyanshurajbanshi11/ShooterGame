using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pre-allocated object pool for floating damage numbers.
/// Listens to CombatEvents.OnHit and spawns pooled numbers at impact locations.
/// </summary>
public class DamageNumberManager : MonoBehaviour
{
    private static DamageNumberManager instance;
    public static DamageNumberManager Instance => instance;

    [Header("Pool Settings")]
    public int poolSize = 30;

    private List<DamageNumber> pool = new List<DamageNumber>();
    private int nextIndex = 0;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        BuildPool();
    }

    void OnEnable()
    {
        CombatEvents.OnHit += HandleHit;
    }

    void OnDisable()
    {
        CombatEvents.OnHit -= HandleHit;
    }

    void BuildPool()
    {
        if (pool.Count > 0) return;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = new GameObject("DamageNumber_" + i);
            obj.transform.SetParent(transform, false);
            DamageNumber dn = obj.AddComponent<DamageNumber>();
            obj.SetActive(false);
            pool.Add(dn);
        }
    }

    private void HandleHit(HitInfo hit)
    {
        SpawnDamageNumber(hit.hitPoint, hit.damage, hit.isHeadshot);
    }

    public void SpawnDamageNumber(Vector3 hitPoint, float damage, bool isHeadshot)
    {
        if (pool.Count == 0) BuildPool();

        // Find next inactive, or recycle using round-robin index
        DamageNumber target = null;
        for (int i = 0; i < pool.Count; i++)
        {
            int checkIndex = (nextIndex + i) % pool.Count;
            if (!pool[checkIndex].gameObject.activeSelf)
            {
                target = pool[checkIndex];
                nextIndex = (checkIndex + 1) % pool.Count;
                break;
            }
        }

        if (target == null)
        {
            // If all busy, recycle current index
            target = pool[nextIndex];
            nextIndex = (nextIndex + 1) % pool.Count;
        }

        target.Spawn(hitPoint, damage, isHeadshot, Camera.main);
    }

    /// <summary>
    /// Auto-initializes an instance in the scene if one doesn't exist yet.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInit()
    {
        if (FindFirstObjectByType<DamageNumberManager>() == null)
        {
            GameObject go = new GameObject("[DamageNumberManager]");
            go.AddComponent<DamageNumberManager>();
            DontDestroyOnLoad(go);
        }
    }
}
