using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GameSystem;

/// <summary>
/// 场景中的道具
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class ItemOnGround : NetworkBehaviour
{
    public ItemSystem.ItemState state = ItemSystem.ItemState.pickable;
    public int ammo;
    public Item item;

    public event System.Action onPicked;

    private void Start()
    {
        if (NetworkSystem.IsServer) NetworkServer.Spawn(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (NetworkSystem.IsServer && state == ItemSystem.ItemState.pickable && other.tag == "Player")
        {
            other.GetComponentInParent<PenguinController>().PickUp(this);
        }
    }

    [ClientRpc]
    public void RpcPicked()
    {
        if (onPicked != null) onPicked();
        NetworkServer.Destroy(gameObject);
    }

    public void Dropped(Vector3 direction, float speed)
    {
        StartCoroutine(Fly(direction, speed));
    }

    private IEnumerator Fly(Vector3 direction, float speed)
    {
        //TODO
        yield return 0;

    }

#if UNITY_EDITOR
    private void Reset()
    {
        SphereCollider collider = GetComponent<SphereCollider>();
        collider.radius = ItemSystem.PickRange;
        collider.isTrigger = true;
    }

    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, ItemSystem.PickRange);
        UnityEditor.Handles.color = Color.white;
    }
#endif
}
