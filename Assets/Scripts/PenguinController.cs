using System.Collections;
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
        Born,
        Walk,
        BeforeSlide,
        Slide,
        Dead
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
            _currentState = value;
        }
    }

    //引用
    public Transform hips;
    public GameObject camPrefab;
    private CinemachineFreeLook virtualCamera;
    public Transform camPoint;
    public Rigidbody[] rids;
    private Animator anim;
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void Start()
    {
        GameSystem.NetworkSystem.playerList.Add(this);
        DontDestroyOnLoad(gameObject);
        if (isLocalPlayer)
        {
            localPenguin = this;
            virtualCamera = Instantiate(camPrefab).GetComponent<Cinemachine.CinemachineFreeLook>();
            StartCoroutine(Born());
            GameSystem.LevelSystem.StartCoroutine(DragView());
        }
        else
        {
            anim.applyRootMotion = false;
        }
    }
    private void OnDestroy()
    {
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

    public void PickUp(ItemOnGround item)
    {
        if (currentItem != null) return;
        currentItem = item;
        item.Picked();
        item.transform.SetParent(itemParent, false);
        gunShot = item.GetComponentInChildren<ParticleSystem>();
        gunShotTimer = 0;
    }

    public void Drop()
    {
        if (currentItem == null) return;
        currentItem.transform.SetParent(null);
        //TODO
    }





    //状态机-----------------------------------------------

    [ContextMenu("Die")]
    public void Die()
    {
        StopAllCoroutines();
        StartCoroutine(_Die());
    }

    private IEnumerator _Die()   //死亡
    {
        CurrentState = PenguinState.Dead;


        //在System上运行协程，这样就可以直接对玩家物体设置Active
        GameSystem.LevelSystem.StartCoroutine(Observe());
        yield break;
    }


    private IEnumerator Observe()   //观战状态
    {
        gameObject.SetActive(false);
        virtualCamera.Follow = LevelSystem.observerPoints[0];
        virtualCamera.LookAt = LevelSystem.observerPoints[0];
        yield break;
    }

    [ContextMenu("Reborn")]
    public void Reborn()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(Born());
    }
    private IEnumerator Born()  //出生
    {
        virtualCamera.Follow = camPoint;
        virtualCamera.LookAt = camPoint;
        StartCoroutine(Walk());
        yield break;
    }



    public float walkSpeed = 1;
    [Header("视野随着转身而旋转的比例")]
    public float viewTurnDragFactor = 0.1f;
    [Command]
    private void CmdWalk()
    {
        RpcWalk();
    }
    [ClientRpc]
    private void RpcWalk()
    {
        anim.enabled = true;
        GetComponent<CharacterController>().enabled = true;
        transform.position = hips.transform.position - Vector3.up * 0.5f;
        hips.transform.localPosition = Vector3.up * 0.5f;
        anim.SetBool("Slide", false);
    }
    private IEnumerator Walk()  //行走状态
    {
        CurrentState = PenguinState.Walk;
        CmdWalk();

        while (true)
        {
            yield return 0;
            //Update here
            if (InputSystem.GetInput(InputCode.BeforeSlide))
            {
                StartCoroutine(BeforeSlide());
                yield break;
            }

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            //rid.AddForce(GetForwardDir() * v * walkForce, ForceMode.Force);
            //rid.AddTorque(Vector3.up * h * walkTorque);


            //计算前进方向
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            camForward.Normalize();   //视线方向归一化向量
            Vector3 forward = (v * camForward - h * Vector3.Cross(camForward, Vector3.up)).normalized;  //前进方向

            float speedAbs = Mathf.Max(Mathf.Abs(v), Mathf.Abs(h)); //速度大小

            float speedForward = Mathf.Clamp01(Vector3.Dot(transform.forward, forward) * walkSpeed * speedAbs);
            float turnSpeed = Vector3.SignedAngle(transform.forward, forward, Vector3.up) / 180;
            //播放行走动画
            //用二次函数模拟缓动
            anim.SetFloat("speed", speedForward * (2 - speedForward));
            //播放转身动画
            //用三次函数模拟双向缓动
            anim.SetFloat("turn", turnSpeed * (2 + turnSpeed) * (2 - turnSpeed));

            //Debug
            Vector3 eyePos = hips.position + Vector3.up * 0.5f;
            Debug.DrawLine(eyePos, eyePos + hips.forward * speedForward * 2, Color.blue);
            Debug.DrawLine(eyePos, eyePos + hips.right * turnSpeed * 2, Color.red);
            Debug.DrawLine(eyePos, eyePos + forward * speedAbs * 2, Color.yellow);
        }
    }




    public float beforeSlideAcceleration;
    public float beforeSlideMaxPower;
    private float beforeSlidePower;
    private IEnumerator BeforeSlide()   //滑行蓄力
    {
        CurrentState = PenguinState.BeforeSlide;
        beforeSlidePower = 0;
        while (true)
        {
            yield return 0;
            //Update here
            if (InputSystem.GetInput(InputCode.Slide))
            {
                StartCoroutine(Slide());
                yield break;
            }
            beforeSlidePower += Time.deltaTime * beforeSlideAcceleration;
            if (beforeSlidePower > beforeSlideMaxPower) beforeSlidePower = beforeSlideMaxPower;
        }
    }




    public float slideForce;
    public float slideTorque;
    public float slideAnimLength = 1;
    [Command]
    private void CmdSlide()
    {
        RpcSlide();
    }
    [ClientRpc]
    private void RpcSlide()
    {
        anim.enabled = false;
        GetComponent<CharacterController>().enabled = false;
        anim.SetBool("Slide", true);
    }
    [Command]
    private void CmdSlide2()
    {
        RpcSlide2();
    }
    [ClientRpc]
    private void RpcSlide2()
    {
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody r in rigidbodies)
        {
            r.velocity = Vector3.zero;
        }
        foreach (Rigidbody r in rids)
        {
            r.AddForce(transform.forward * (slideForce + Mathf.Log(beforeSlidePower, 2)), ForceMode.Impulse);
        }
    }
    private IEnumerator Slide() //滑行状态
    {
        CurrentState = PenguinState.Slide;

        CmdSlide();
        yield return new WaitForSeconds(slideAnimLength);
        CmdSlide2();

        while (true)
        {
            yield return new WaitForFixedUpdate();
            //Update here

            if (InputSystem.GetInput(InputCode.Walk))
            {
                StartCoroutine(Walk());
                yield break;
            }
        }
    }

    private IEnumerator Func()
    {
        while (true)
        {
            yield return 0;
            if (currentItem == null) continue;
            if (InputSystem.GetInput(InputCode.Func))
            {
                switch (currentItem.theItem.type)
                {
                    case ItemType.Gun:
                        while (gunShotTimer > currentItem.theItem.deltaTime)
                        {
                            gunShotTimer -= currentItem.theItem.deltaTime;
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
                if (gunShotTimer < currentItem.theItem.deltaTime)
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

    private IEnumerator DragView()
    {
        while (true)
        {
            yield return 0;
            float x = 0;
            float y = 0;
            if (Input.GetMouseButton(0))
            {
                x = Input.GetAxis("Mouse X");
                y = Input.GetAxis("Mouse Y");
            }
            virtualCamera.m_XAxis.m_InputAxisValue = Input.GetMouseButton(0) ? x : -Input.GetAxis("Horizontal") * viewTurnDragFactor;
            virtualCamera.m_YAxis.m_InputAxisValue = y;
        }
    }

    public HarmSystem.HarmInformation GetHarmInformation(Collision collision)
    {
        return new HarmSystem.HarmInformation();
    }
}
