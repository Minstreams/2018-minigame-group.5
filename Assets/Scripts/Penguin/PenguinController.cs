using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;
using UnityEngine.Networking;
using Cinemachine;


/// <summary>
/// 企鹅控制器
/// </summary>
public class PenguinController : HarmSystem.HitTarget
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

    public bool isDead { get; set; }

    [Command]
    private void CmdSetCurrentState(PenguinState penguinState)
    {
        //RpcSetCurrentState(penguinState);
        _currentState = penguinState;
    }
    //[ClientRpc]
    //private void RpcSetCurrentState(PenguinState penguinState)
    //{
        
    //}

    //引用
    public Transform hips;  //bone root
    [ContextMenu("ResetHips")]
    private void ResetHips()
    {
        posRecorder.Read();
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
    public Rigidbody[] rigidbodies;
    private AudioSource audioSource;
    private void Awake()
    {
        Debug.Log("Penguin Awake!");
        anim = GetComponent<Animator>();
        sync = GetComponent<SyncLerpMove>();
        posRecorder = GetComponent<PosRecorder>();
        posShaper = GetComponent<PosShaper>();
        simpleGravity = GetComponent<SimpleGravity>();
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
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

    public void PlayAudio(AudioClip clip)
    {
        audioSource.clip = clip;
        if (clip != null)
            audioSource.Play();
    }


    //道具------------------------------------------------
    public Transform itemParent;
    public ItemOnGround currentItem;

    private Vector3 GetForwardDir()
    {
        return transform.forward;
    }

    [Server]
    public void PickUp(ItemOnGround item)
    {
        if (currentItem != null) return;
        RpcPickUp(new ItemSystem.ItemGenerationInformation(ItemSystem.ItemState.onHand, item.ammo, item.item));
    }

    [ClientRpc]
    public void RpcPickUp(ItemSystem.ItemGenerationInformation information)
    {
        if (debug) Debug.Log("RpcPickUp");
        currentItem = ItemSystem.GenerateItem(itemParent, information);
        currentItem.owner = this;
        PlayAudio(currentItem.item.pickUp);
    }

    public void Drop()
    {
        currentItem.transform.SetParent(null);
        currentItem.Drop(aimPos);
        currentItem = null;
    }


    private void OnAnimatorIK(int layerIndex)
    {
        bool ik = currentItem != null;
        //视线位置
        anim.SetLookAtWeight(ik ? 0.8f : 0, 0.5f, 1, 0, 1);
        anim.SetLookAtPosition(aimPos);

        //右手位置
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, ik ? 0.4f : 0);
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, ik ? 0.7f : 0);
        anim.SetIKPosition(AvatarIKGoal.RightHand, aimPos);
        anim.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.LookRotation(aimPos - transform.position - Vector3.up * 0.5f));
    }






    //状态机-----------------------------------------------
    private bool slideButton;
    private bool brakeButton;
    private bool dropButton;
    [SyncVar]
    private Vector3 aimPos;
    [Command]
    public void CmdRecordInputAxis(float sf, float ts, bool sb, bool bb, bool fb, Vector3 ap, bool drop)
    {
        RpcRecordInputAxis(sf, ts, sb, bb, fb, ap, drop);
    }
    [ClientRpc]
    public void RpcRecordInputAxis(float sf, float ts, bool sb, bool bb, bool fb, Vector3 ap, bool drop)
    {
        slideButton = sb;
        brakeButton = bb;
        aimPos = ap;

        if (!isLocalPlayer && !isDead)
        {
            //播放行走动画
            //用二次函数模拟缓动
            anim.SetFloat("speed", sf * (2 - sf));
            //播放转身动画
            //用三次函数模拟双向缓动
            anim.SetFloat("turn", ts * (2 + ts) * (2 - ts));

            if (currentItem != null)
            {
                if (fb) currentItem.Func();
                if (drop) Drop();
            }
        }
    }
    LinkedListNode<Coroutine> inputCoroutine = null;
    private IEnumerator RecordInputAxis()
    {
        while (true)
        {
            yield return 0;

            float h = InputSystem.GetHorizontalAxis();
            float v = InputSystem.GetVeticalAxis();


            //计算前进方向
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            camForward.Normalize();   //视线方向归一化向量
            Vector3 forward = (v * camForward - h * Vector3.Cross(camForward, Vector3.up)).normalized;  //前进方向

            float speedAbs = Mathf.Max(Mathf.Abs(v), Mathf.Abs(h)); //速度大小



            float sf = Mathf.Clamp01(Vector3.Dot(transform.forward, forward) * walkSpeed * speedAbs);   //speed forward
            float ts = Vector3.SignedAngle(transform.forward, forward, Vector3.up) / 180;   //turn speed
            bool fb = InputSystem.GetInput(InputCode.Func); // func button
            Vector3 ap = Camera.main.transform.position + Camera.main.transform.forward * 7;
            //ap.y = ap.y * 0.5f + 0.25f;

            bool drop = InputSystem.GetInput(InputCode.Drop);

            if (!isDead)
            {
                //播放行走动画
                //用二次函数模拟缓动
                anim.SetFloat("speed", sf * (2 - sf));
                //播放转身动画
                //用三次函数模拟双向缓动
                anim.SetFloat("turn", ts * (2 + ts) * (2 - ts));
                if (currentItem != null)
                {
                    if (fb) currentItem.Func();
                    if (drop) Drop();
                }
            }
            if (isServer)
            {
                slideButton = InputSystem.GetInput(InputCode.Slide);
                brakeButton = InputSystem.GetInput(InputCode.Brake);
                aimPos = ap;
            }
            else
            {
                CmdRecordInputAxis(sf, ts, InputSystem.GetInput(InputCode.Slide), InputSystem.GetInput(InputCode.Brake), fb, ap, drop);
            }
            //Debug
            if (debug)
            {
                Vector3 eyePos = hips.position + Vector3.up * 0.5f;
                Debug.DrawLine(eyePos, eyePos + hips.forward * sf * 2, Color.blue);
                Debug.DrawLine(eyePos, eyePos + hips.right * ts * 2, Color.red);
                Debug.DrawLine(eyePos, eyePos + forward * speedAbs * 2, Color.yellow);
            }
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
        if (currentItem != null)
        {
            Destroy(currentItem.gameObject);
            currentItem = null;
        }
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



    //Walk~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~!
    public float walkSpeed = 1;
    public Vector3 walkStartHipsOffset;
    [ClientRpc]
    private void RpcWalk()
    {
        anim.enabled = true;
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
        anim.SetBool("Brake", false);
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
        foreach (Rigidbody r in rigidbodies)
        {
            r.velocity = Vector3.zero;
        }
        if (debug) Debug.Log("Push[slideForce:" + slideForce + "][power:" + power + "][PowPower:" + Mathf.Pow(power, powerPoint) + "]");
        foreach (Rigidbody r in rids)
        {
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
                Vector3 v = hips.GetComponent<Rigidbody>().velocity + Vector3.up * 2.5f - posShaper.yRot * Vector3.forward * 1.5f;
                if (debug) Debug.Log("Trying to Stand up[velocity:" + v + v.magnitude + "][flip:" + posShaper.flip + "]");
                if (!posShaper.flip && v.magnitude < minStandSpeed)
                {
                    anim.SetBool("Brake", true);
                    StartCoroutine(Walk());

                    yield return new WaitForSeconds(0.2f);
                    anim.SetBool("Slide", false);
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
    public float causeForce;
    public float causeHarm;
    public float causeDestroyPower;

    public override void OnServerHarm(HarmSystem.HarmInformation information)
    {
        RpcOnHarm(information);

    }

    [ClientRpc]
    public void RpcOnHarm(HarmSystem.HarmInformation information)
    {
        Debug.Log(this.name + " is hitted.[result:" + information.direction * information.force * harm * 0.01f * HarmSystem.HarmFactor + "] [force:" + information.force + "][harm:" + information.harm + "][destroyPower:" + information.destroyPower + "][direction:" + information.direction + "]");
        harm += information.harm;
        HarmSystem.ShowFloatingNumber(information.force + " * " + harm + "%", information.position);
        ImpulseSpeed(information.direction * information.force * harm * 0.01f * HarmSystem.HarmFactor);
    }

    [Server]
    public void ConstantSpeed(Vector3 s)
    {
        RpcConstantSpeed(s);
    }

    [ClientRpc]
    private void RpcConstantSpeed(Vector3 s)
    {
        if (debug) Debug.Log("RpcConstantSpeed[s:" + s + "]");
        switch (CurrentState)
        {
            case PenguinState.Walk:
            case PenguinState.BeforeSlide:
                transform.Translate(s * Time.deltaTime, Space.World);
                break;
            case PenguinState.Slide:
            case PenguinState.Brake:
                foreach (Rigidbody r in rigidbodies)
                {
                    r.AddForce(s, ForceMode.Acceleration);
                }
                break;
        }
    }

    [Server]
    public void ImpulseSpeed(Vector3 s)
    {
        RpcImpulseSpeed(s);
    }

    public float impulseTolerence = 10;
    [ClientRpc]
    private void RpcImpulseSpeed(Vector3 s)
    {
        if (debug) Debug.Log("RpcImpulseSpeed[s:" + s + "]");
        switch (CurrentState)
        {
            case PenguinState.Walk:
            case PenguinState.BeforeSlide:
                if (s.magnitude < impulseTolerence)
                {
                    //效果？
                    break;
                }
                anim.SetBool("Slide", true);
                anim.SetBool("Brake", false);
                pushRecorder.Record();
                anim.enabled = false;
                pushFormer.Read();
                pushRecorder.Read();
                simpleGravity.enabled = false;
                foreach (Rigidbody r in rigidbodies)
                {
                    r.velocity = Vector3.zero;
                }
                sync.syncRigidbody = true;
                if (NetworkSystem.IsServer) { StopAllCoroutines(); StartCoroutine(Slide()); }
                break;
            default:
                foreach (Rigidbody r in rigidbodies)
                {
                    r.AddForce(s, ForceMode.VelocityChange);
                }
                break;
        }

    }
}
