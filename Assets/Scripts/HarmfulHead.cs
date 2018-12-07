using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;

/// <summary>
/// 放到小企鹅头上
/// </summary>
public class HarmfulHead : MonoBehaviour
{
    PenguinController penguin;

    void Awake()
    {
        penguin = GetComponentInParent<PenguinController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (NetworkSystem.IsServer && penguin.CurrentState == PenguinController.PenguinState.Slide && collision.collider.CompareTag("Player"))
        {
            collision.collider.GetComponent<PenguinController>().OnServerHarm(new HarmSystem.HarmInformation(penguin.causeForce, penguin.causeHarm, penguin.causeDestroyPower, -collision.impulse.normalized, collision.contacts[0].point));
        }
    }
}
