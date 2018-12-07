﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;
using UnityEngine.Networking;
using Cinemachine;


/// <summary>
/// 企鹅控制器
/// </summary>
public class PenguinController : HarmSystem.HitTarget, HarmSystem.FlyingAmmo
{
    //三层结构
    //底层：企鹅状态切换&动画播放&道具系统
    //中层：联网控制&物理效果(可以协程控制)
    //高层：交互封装


    //底层-----------------------------------------------
    public static PenguinController localPenguin { get; private set; }

    public bool debug = false;

    public enum PenguinState
    {
        Walk,
        BeforeSlide,
        Slide,
        Brake
    }
    [SyncVar]
    private PenguinState _currentState = PenguinState.Walk;
    /// <summary>
    /// 当前状态,用于外部查询
    /// </summary>
    public PenguinState CurrentState
    {
        get { return _currentState; }
        private set
        {
            if (_currentState == value) return;
            if (debug) Debug.Log(gameObject + " change state to " + value);
            CmdSetCurrentState(value);
        }
    }

    public bool isDead { get; private set; }

    [Command]
    private void CmdSetCurrentState(PenguinState penguinState)
    {
        RpcSetCurrentState(penguinState);
    }
    [ClientRpc]
    private void RpcSetCurrentState(PenguinState penguinState)
    {
        _currentState = penguinState;
    }

    //引用
    public Transform hips;  //bone root
    [ContextMenu("ResetHips")]
    private void ResetHips()
    {
        posRecorder.Read();
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody r in rigidbodies)
        {
            r.velocity = Vector3.zero;
            //r.Sleep();
        }
    }
    public Transform camPoint;  //camera focus point
    public Rigidbody[] rids;    //rigidbodies pushed when slide 
    private Animator anim;
    private SyncLerpMove sync;
    private SimpleGravity simpleGravity;
    private PosRecorder posRecorder;
    private PosShaper posShaper;
    private void Awake()
    {
        Debug.Log("Penguin Awake!");
        anim = GetComponent<Animator>();
        sync = GetComponent<SyncLerpMove>();
        posRecorder = GetComponent<PosRecorder>();
        posShaper = GetComponent<PosShaper>();
        simpleGravity = GetComponent<SimpleGravity>();
        DontDestroyOnLoad(gameObject);
        NetworkSystem.playerList.Add(this);
    }
    private void Start()
    {
        if (isLocalPlayer)
        {
            localPenguin = this;
            inputCoroutine = LevelSystem.StartCoroutine(RecordInputAxis());
            CmdReborn();
        }
        else
        {
            switch (CurrentState)
            {
                case PenguinState.Walk:
                case PenguinState.BeforeSlide:
                    anim.enabled = true;
                    simpleGravity.enabled = true;
                    break;
                case PenguinState.Slide:
                case PenguinState.Brake:
                    anim.enabled = false;
                    simpleGravity.enabled = false;
                    break;
            }
        }
    }
    private void OnDestroy()
    {
        LevelSystem.StopCoroutine(inputCoroutine);
        GameSystem.NetworkSystem.playerList.Remove(this);
    }




    //道具------------------------------------------------
    public Transform itemParent;
    private ItemOnGround currentItem;
    private ParticleSystem gunShot;
    private float gunShotTimer;

    private Vector3 GetForwardDir()
    {
        return transform.forward;
    }

    [Server]
    public void PickUp(ItemOnGround item)
    {
        if (currentItem != null) return;
        RpcPickUp(new ItemSystem.ItemGenerationInformation(false, ItemSystem.ItemState.onHand, item.ammo, item.item));
        item.RpcPicked();
    }

    [ClientRpc]
    public void RpcPickUp(ItemSystem.ItemGenerationInformation information)
    {
        currentItem = ItemSystem.GenerateItem(itemParent, information);
        gunShot = currentItem.GetComponentInChildren<ParticleSystem>();
        gunShotTimer = 0;
    }

    public void Drop()
    {
        if (currentItem == null) return;
        currentItem.transform.SetParent(null);
        //TODO
    }

    private IEnumerator Func()
    {
        while (true)
        {
            yield return 0;
            if (currentItem == null) continue;
            if (InputSystem.GetInput(InputCode.Func))
            {
                switch (currentItem.item.type)
                {
                    case ItemType.Gun:
                        while (gunShotTimer > currentItem.item.deltaTime)
                        {
                            gunShotTimer -= currentItem.item.deltaTime;
                            gunShot.Emit(1);
                        }
                        gunShotTimer += Time.deltaTime;
                        break;
                    case ItemType.Rock:
                    case ItemType.Stick:
                        break;
                }
            }
            else
            {
                if (gunShotTimer < currentItem.item.deltaTime)
                {
                    gunShotTimer += Time.deltaTime;
                }
            }
            if (InputSystem.GetInput(InputCode.Drop))
            {
                Drop();
            }
        }
    }




    //状态机-----------------------------------------------
    private float speedForward;
    private float turnSpeed;
    private bool slideButton;
    private bool brakeButton;
    [Command]
    public void CmdRecordInputAxis(float sf, float ts, bool sb, bool bb)
    {
        RpcRecordInputAxis(sf, ts, sb, bb);
    }
    [ClientRpc]
    public void RpcRecordInputAxis(float sf, float ts, bool sb, bool bb)
    {
        speedForward = sf;
        turnSpeed = ts;
        slideButton = sb;
        brakeButton = bb;

        if (!isLocalPlayer)
        {
            //播放行走动画
            //用二次函数模拟缓动
            anim.SetFloat("speed", sf * (2 - sf));
            //播放转身动画
            //用三次函数模拟双向缓动
            anim.SetFloat("turn", ts * (2 + ts) * (2 - ts));
        }
    }
    LinkedListNode<Coroutine> inputCoroutine = null;
    private IEnumerator RecordInputAxis()
    {
        while (true)
        {
            yield return 0;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");


            //计算前进方向
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            camForward.Normalize();   //视线方向归一化向量
            Vector3 forward = (v * camForward - h * Vector3.Cross(camForward, Vector3.up)).normalized;  //前进方向

            float speedAbs = Mathf.Max(Mathf.Abs(v), Mathf.Abs(h)); //速度大小



            float sf = Mathf.Clamp01(Vector3.Dot(transform.forward, forward) * walkSpeed * speedAbs);
            float ts = Vector3.SignedAngle(transform.forward, forward, Vector3.up) / 180;

            //播放行走动画
            //用二次函数模拟缓动
            anim.SetFloat("speed", sf * (2 - sf));
            //播放转身动画
            //用三次函数模拟双向缓动
            anim.SetFloat("turn", ts * (2 + ts) * (2 - ts));
            if (isServer)
            {
                slideButton = InputSystem.GetInput(InputCode.Slide);
                brakeButton = InputSystem.GetInput(InputCode.Brake);
                speedForward = sf;
                turnSpeed = ts;

            }
            else
            {
                CmdRecordInputAxis(sf, ts, InputSystem.GetInput(InputCode.Slide), InputSystem.GetInput(InputCode.Brake));
            }
            //Debug
            Vector3 eyePos = hips.position + Vector3.up * 0.5f;
            Debug.DrawLine(eyePos, eyePos + hips.forward * speedForward * 2, Color.blue);
            Debug.DrawLine(eyePos, eyePos + hips.right * turnSpeed * 2, Color.red);
            Debug.DrawLine(eyePos, eyePos + forward * speedAbs * 2, Color.yellow);
        }
    }


    [ClientRpc]
    public void RpcDie() //Called by dead zone
    {
        isDead = true;
        LevelSystem.RpcDie(this);
    }

    [ClientRpc]
    public void RpcReborn(Vector3 position) //Called by Level System
    {
        isDead = false;
        LevelSystem.RpcReborn(this, position);
        simpleGravity.Init();
        simpleGravity.enabled = false;
        anim.enabled = false;
        gameObject.SetActive(false);
        ResetHips();
        gameObject.SetActive(true);


        if (NetworkSystem.IsServer) StartCoroutine(Slide());
    }
    [Command]
    public void CmdReborn() //Called by the Matrix
    {
        Debug.Log("CmdReborn");
        RpcReborn(LevelSystem.GetNextStartPoint());
    }

    [ClientRpc]
    public void RpcLoadScene(int index)
    {
        LevelSystem.RpcLoadScene(index);
    }


    //Walk~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~!
    public float walkSpeed = 1;
    public Vector3 walkStartHipsOffset;
    [ClientRpc]
    private void RpcWalk()
    {
        anim.enabled = true;
        anim.SetBool("Slide", false);
        simpleGravity.enabled = true;
        sync.syncRigidbody = false;
        {
            //reset angle
            Vector3 euler = hips.rotation.eulerAngles;
            euler.z = euler.x = 0;
            Quaternion tRot = Quaternion.Euler(euler);
            sync.SetSyncVar(hips.transform.position - tRot * walkStartHipsOffset, tRot);
            hips.transform.localPosition = walkStartHipsOffset;
        }
        sync.ApplySyncVar();

    }
    private IEnumerator Walk()  //行走状态
    {
        CurrentState = PenguinState.Walk;
        RpcWalk();

        while (true)
        {
            yield return 0;
            //Update here
            if (slideButton)
            {
                StartCoroutine(BeforeSlide());
                yield break;
            }

        }
    }





    //BeforeSlide~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public float beforeSlideAcceleration;
    public float beforeSlideMaxPower;
    private float beforeSlidePower;
    public float powerPoint = 1.3f;

    public PosRecorder pushFormer;
    public RidRecorder pushRecorder;
    [ClientRpc]
    private void RpcBeforeSlide() //Init here
    {
        anim.SetBool("Slide", true);
    }
    [Command]
    private void CmdPush(float power)
    {
        RpcPush(power);
    }
    [ClientRpc]
    private void RpcPush(float power)
    {
        pushRecorder.Record();
        anim.enabled = false;
        pushFormer.Read();
        pushRecorder.Read();
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody r in rigidbodies)
        {
            r.velocity = Vector3.zero;
        }
        foreach (Rigidbody r in rids)
        {
            if (debug) Debug.Log("Push[slideForce:" + slideForce + "][power:" + power + "][PowPower:" + Mathf.Pow(power, powerPoint) + "]");
            r.AddForce(transform.forward * (slideForce + Mathf.Pow(power, powerPoint)), ForceMode.Impulse);
        }
    }
    private IEnumerator BeforeSlide()   //滑行蓄力
    {
        CurrentState = PenguinState.BeforeSlide;
        beforeSlidePower = 1;
        RpcBeforeSlide();
        while (true)
        {
            yield return 0;
            //Update here
            if (!slideButton)
            {
                CmdPush(beforeSlidePower);
                StartCoroutine(Slide());
                yield break;
            }
            beforeSlidePower += Time.deltaTime * beforeSlideAcceleration;
            if (beforeSlidePower > beforeSlideMaxPower) beforeSlidePower = beforeSlideMaxPower;
        }
    }






    //Slide~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public float slideForce;
    public float slideAnimLength = 1;

    [ClientRpc]
    private void RpcSlide() //Init here
    {
        simpleGravity.enabled = false;
        sync.syncRigidbody = true;
    }
    private IEnumerator Slide() //滑行状态
    {
        CurrentState = PenguinState.Slide;

        RpcSlide();

        while (true)
        {
            yield return new WaitForFixedUpdate();
            //Update here

            if (brakeButton)
            {
                StartCoroutine(Brake());
                yield break;
            }
        }
    }



    //Brake~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public float minStandSpeed = 5;
    [ClientRpc]
    private void RpcBrake() //Init here
    {

    }
    private IEnumerator Brake() //滑行状态
    {
        CurrentState = PenguinState.Brake;

        posShaper.RpcStartShape();
        RpcBrake();

        while (true)
        {
            yield return new WaitForFixedUpdate();
            //Update here

            if (!brakeButton)
            {
                posShaper.RpcEndShape();
                if (debug) Debug.Log("Trying to Stand up[velocity:" + hips.GetComponent<Rigidbody>().velocity.magnitude + "][flip:" + posShaper.flip + "]");
                if (!posShaper.flip && hips.GetComponent<Rigidbody>().velocity.magnitude < minStandSpeed)
                {
                    anim.SetBool("Slide", false);
                    StartCoroutine(Walk());
                }
                else
                {
                    StartCoroutine(Slide());
                }
                yield break;
            }
        }
    }

    //伤害计算
    public float harm = 0;
    public float moveForceFactor = 5f;
    public HarmSystem.HarmInformation GetHarmInformation(Collision collision)
    {
        switch (CurrentState)
        {
            case PenguinState.Walk:
            case PenguinState.BeforeSlide:
                return new HarmSystem.HarmInformation(100, 0, 0, collision.impulse.normalized, collision.contacts[0].point);
            case PenguinState.Slide:
            case PenguinState.Brake:
                return new HarmSystem.HarmInformation(collision.impulse.magnitude * moveForceFactor, collision.impulse.magnitude * 100, 0, collision.relativeVelocity.normalized, collision.contacts[0].point);
            default:
                return new HarmSystem.HarmInformation();
        }
    }

    public override void OnServerHarm(HarmSystem.HarmInformation information)
    {
        RpcOnHarm(information);

    }

    [ClientRpc]
    public void RpcOnHarm(HarmSystem.HarmInformation information)
    {
        Debug.Log(this.name + " is hitted. [force:" + information.force + "][harm:" + information.harm + "][destroyPower:" + information.destroyPower + "][direction:" + information.direction + "]");
        switch (CurrentState)
        {
            case PenguinState.Walk:
            case PenguinState.BeforeSlide:
            case PenguinState.Slide:
            case PenguinState.Brake:
            default:
                harm += information.harm;
                HarmSystem.ShowFloatingNumber(information.force.ToString(), information.position);
                hips.GetComponent<Rigidbody>().AddForce(information.direction * information.force * harm * HarmSystem.HarmFactor, ForceMode.Impulse);
                break;
        }
    }
}
