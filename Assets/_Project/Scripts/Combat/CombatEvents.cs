using System;
using UnityEngine;

/// <summary>
/// Data payload for shot impact events.
/// Decouples gun logic from UI feedback (hitmarkers, damage numbers, audio).
/// </summary>
public struct HitInfo
{
    public Vector3 hitPoint;
    public float damage;
    public bool isHeadshot;
    public bool isKill;
    public IDamageable target;
}

/// <summary>
/// Global combat event bus. Gun only notifies this bus, keeping weapons decoupled from UI.
/// </summary>
public static class CombatEvents
{
    public static event Action<HitInfo> OnHit;

    public static void TriggerHit(Vector3 hitPoint, float damage, bool isHeadshot, bool isKill = false, IDamageable target = null)
    {
        OnHit?.Invoke(new HitInfo
        {
            hitPoint = hitPoint,
            damage = damage,
            isHeadshot = isHeadshot,
            isKill = isKill,
            target = target
        });
    }
}
