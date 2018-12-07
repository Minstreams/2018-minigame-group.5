using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;

[RequireComponent(typeof(AudioSource))]
public class ItemGun : MonoBehaviour
{

    private ParticleSystem part;
    private ItemOnGround item;
    [HideInInspector]
    public AudioSource asource;
    public List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    public int ammoNum = 1;

    void Awake()
    {
        part = GetComponent<ParticleSystem>();
        item = GetComponentInParent<ItemOnGround>();
        asource = GetComponent<AudioSource>();
    }

    void OnParticleCollision(GameObject other)
    {
        HarmSystem.HitTarget target = other.GetComponentInParent<HarmSystem.HitTarget>();
        if (target == null || !target.CompareTag("Player")) return;

        part.GetCollisionEvents(other, collisionEvents);
        item.OnAmmoHit(target, collisionEvents[0].velocity.normalized, collisionEvents[0].intersection);

        //int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

        //Rigidbody rb = other.GetComponent<Rigidbody>();
        //int i = 0;

        //while (i < numCollisionEvents)
        //{
        //    if (rb)
        //    {
        //        Vector3 pos = collisionEvents[i].intersection;
        //        Vector3 force = collisionEvents[i].velocity * 10;
        //        rb.AddForce(force);
        //    }
        //    i++;
        //}
    }


    public void Fire()
    {
        part.Emit(ammoNum);
        if (asource.clip != null)
            asource.Play();
    }
}
