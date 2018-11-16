﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存储道具数据
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "道具")]
public class Item : ScriptableObject
{
    public GameObject prefab;
    public Vector3 offset;
    public ItemType type;
    public float force;
    public float harm;
    public float destroyPower;
    public float speed;
    [Header("开火间隔(s)")]
    public float deltaTime;
    public AudioClip sound;

    public float ammoForce;
    public float ammoHarm;
    public float ammoDestroyPower;
}

public enum ItemType
{
    Stick,
    Rock,
    Gun
}