﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;

/// <summary>
/// 场景中的道具
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class ItemOnGround : MonoBehaviour
{
    public bool debug = false;
    public ItemSystem.ItemState state = ItemSystem.ItemState.pickable;
    public int ammo;
    public Item item;

    public event System.Action onPicked;

    private void Awake()
    {
        gun = GetComponentInChildren<ItemGun>();
        model = GetComponentInChildren<ItemModel>();
    }
    private void Start()
    {
        if (gun != null)
            gun.asource.clip = item.sound;
        if (state == ItemSystem.ItemState.pickable) StartCoroutine(Rotate());
    }








    //Pickable~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private static Vector3 rotEuler = new Vector3(10, 12, 0);
    private bool isPicked = false;
    private IEnumerator Rotate()
    {
        while (true)
        {
            transform.Rotate(rotEuler * Time.deltaTime);
            yield return 0;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (state == ItemSystem.ItemState.pickable && other.tag == "Player" && !isPicked)
        {
            if (NetworkSystem.IsServer) other.GetComponentInParent<PenguinController>().PickUp(this);
            if (onPicked != null) onPicked();
            Destroy(gameObject);
        }
    }






    //OnHand~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private ItemGun gun;
    private ItemModel model;
    private float lastFuncTime;
    private float timer;
    private PenguinController _owner;
    public PenguinController owner
    {
        private get { return _owner; }
        set
        {
            _owner = value;
            Collider c = model.GetComponent<Collider>();
            foreach (Rigidbody r in value.rigidbodies)
            {
                Physics.IgnoreCollision(r.GetComponent<Collider>(), c);
            }
        }
    }

    public void Func()
    {
        if (debug) Debug.Log("Func!");
        switch (item.type)
        {
            case ItemType.Gun:
                timer = Time.time;
                if (timer - lastFuncTime >= item.deltaTime)
                {
                    lastFuncTime = timer;
                    gun.Fire();
                    model.BackPower();
                    if (debug) Debug.Log("Fire!");
                }

                break;

        }
    }


    //CalledByItemGun
    public void OnAmmoHit(HarmSystem.HitTarget target, Vector3 direction, Vector3 position)
    {
        if (NetworkSystem.IsServer)
            target.OnServerHarm(new HarmSystem.HarmInformation(item.ammoForce, item.ammoHarm, item.ammoDestroyPower, direction, position));
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
