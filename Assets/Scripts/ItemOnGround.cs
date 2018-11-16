using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 场景中的道具
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class ItemOnGround : MonoBehaviour
{
    public const float pickRange = 1;
    public enum ItemState
    {
        pickable,
        rock,
        onHand
    }
    private ItemState state = ItemState.pickable;

    public int ammo;
    public Item theItem;

    public event System.Action onPicked;

    private void OnTriggerEnter(Collider other)
    {
        if (state == ItemState.pickable && other.tag == "Player")
        {
            other.GetComponent<PenguinController>().PickUp(this);
        }
    }

    public void Picked()
    {
        state = ItemState.onHand;
        if (onPicked != null) onPicked();
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
        collider.radius = pickRange;
        collider.isTrigger = true;
    }

    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.color = Color.blue;
        UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, pickRange);
        UnityEditor.Handles.color = Color.white;
    }
#endif
}
