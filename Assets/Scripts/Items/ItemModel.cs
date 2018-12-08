using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class ItemModel : MonoBehaviour
{
    private ItemOnGround item;
    private Vector3 startLocalPos;
    public float returnRate;
    public Vector3 backPower;

    void Awake()
    {
        item = GetComponentInParent<ItemOnGround>();
        startLocalPos = transform.localPosition;
    }

    void FixedUpdate()
    {
        if (!isRock)
            transform.localPosition += returnRate * (startLocalPos - transform.localPosition);
    }

    public void BackPower()
    {
        transform.localPosition += backPower;
    }

    //Rock
    private bool isRock = false;
    public void StartAsRock()
    {
        isRock = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isRock) return;
        if (collision.collider.CompareTag("Player"))
        {
            PenguinController penguin = collision.collider.GetComponentInParent<PenguinController>();
            if (penguin == item.owner)
            {
                return;
            }
            else if (NetworkSystem.IsServer)
            {
                penguin.OnServerHarm(new HarmSystem.HarmInformation(item.item.force, item.item.harm, item.item.destroyPower, GetComponent<Rigidbody>().velocity.normalized, collision.contacts[0].point));
                StopRock();
            }
        }
        else
        {
            if (item.owner != null)
            {
                item.owner = null;
                return;
            }
            else
            {
                StopRock();
            }
        }
    }
    public void StopRock()
    {
        isRock = false;
        GetComponent<Rigidbody>().isKinematic = true;
        item.transform.position = transform.position - item.transform.rotation * startLocalPos;
        transform.localPosition = startLocalPos;
        item.BecomePickable();
    }
}
