using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        transform.localPosition += returnRate * (startLocalPos - transform.localPosition);
    }

    public void BackPower()
    {
        transform.localPosition += backPower;
    }


}
