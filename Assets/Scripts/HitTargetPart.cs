using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 放在小企鹅所有Rigid Body上
/// </summary>
public class HitTargetPart : MonoBehaviour
{
    [HideInInspector]
    public PenguinController penguin;
    void Awake()
    {
        penguin = GetComponentInParent<PenguinController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!GameSystem.NetworkSystem.IsServer) return;
        GameSystem.HarmSystem.FlyingAmmo ammo = collision.collider.GetComponentInParent<GameSystem.HarmSystem.FlyingAmmo>();
        if (ammo != null && (Object)ammo != penguin)
        {
            penguin.OnServerHarm(ammo.GetHarmInformation(collision));
        }
    }
}
