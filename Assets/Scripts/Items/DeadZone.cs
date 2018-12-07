using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DeadZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (GameSystem.NetworkSystem.IsServer && other.CompareTag("Player"))
        {
            PenguinController penguin = other.GetComponentInParent<PenguinController>();
            if (!penguin.isDead) { penguin.isDead = true; penguin.RpcDie(); }
        }
    }

    private void OnDrawGizmos()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCollider.center + transform.position, Vector3.Scale(boxCollider.size, transform.localScale));
        Gizmos.color = Color.white;
    }
}
