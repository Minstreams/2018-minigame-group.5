using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;
using UnityEngine.Networking;
using Cinemachine;


/// <summary>
/// 企鹅控制器
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PenguinController : HarmSystem.HitTarget, HarmSystem.FlyingAmmo
{
    //三层结构
    //底层：企鹅状态切换&动画播放&道具系统
    //中层：联网控制&物理效果(可以协程控制)
    //高层：交互封装


    //底层-----------------------------------------------
    public enum PenguinState
    {
        Walk,
        BeforeSlide,
        Slide
    }
    private PenguinState _currentState = PenguinState.Walk;
    public PenguinState currentState
    {
        get { return _currentState; }
        set
        {
            if (_currentState == value) return;
            switch (value)
            {
                //TODO：动画播放
                case PenguinState.Walk:
                    hips.Rotate(Vector3.left * 90, Space.Self);
                    Debug.Log("Walk!");
                    break;
                case PenguinState.BeforeSlide:
                    Debug.Log("BeforeSlide!");
                    break;
                case PenguinState.Slide:
                    hips.Rotate(Vector3.left * -90, Space.Self);
                    Debug.Log("Slide!");
                    break;
                default:
                    break;
            }
            _currentState = value;
        }
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





    //中层-----------------------------------------------
    private Rigidbody rid;
    private Animator anim;
    public Transform hips;
    public GameObject camPrefab;
    public float walkSpeed = 1;
    public float walkTorque;
    public float walkDrag;

    public float beforeSlideDrag;
    public float beforeSlideAcceleration;
    public float beforeSlideMaxPower;
    private float beforeSlidePower;

    public float slideForce;
    public float slideTorque;
    public float slideDrag;

    CinemachineFreeLook virtualCamera;
    private void Awake()
    {
        rid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        if (isLocalPlayer)
        {
            virtualCamera = Instantiate(camPrefab).GetComponent<Cinemachine.CinemachineFreeLook>();
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform.GetChild(2);
            StartCoroutine(Walk());
            StartCoroutine(Func());
            StartCoroutine(DragView());
        }
    }

    /// <summary>
    /// 前进速度
    /// </summary>
    float speedForward;
    /// <summary>
    /// 转身速度
    /// </summary>
    float turnSpeed;
    [Header("视野随着转身而旋转的比例")]
    public float viewTurnDragFactor = 0.1f;

    private IEnumerator Walk()
    {
        currentState = PenguinState.Walk;
        anim.SetBool("Slide", false);
        anim.applyRootMotion = true;

        rid.drag = walkDrag;
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

            float angle = Vector3.SignedAngle(transform.forward, forward, Vector3.up);  //前进转角

            float speedAbs = Mathf.Max(Mathf.Abs(v), Mathf.Abs(h)); //速度大小

            speedForward = Vector3.Dot(transform.forward, forward) * walkSpeed * speedAbs;
            turnSpeed = Vector3.SignedAngle(transform.forward, forward, Vector3.up) / 180 + Vector3.Dot(transform.right, forward);
            //播放转身动画
            anim.SetFloat("turn", turnSpeed);

            //播放行走动画
            anim.SetFloat("speed", Mathf.Sin(Mathf.Clamp01(speedForward) * Mathf.PI * 0.5f));


            Vector3 eyePos = transform.position + Vector3.up * 0.5f;
            Debug.DrawLine(eyePos, eyePos + transform.forward * speedForward * 2, Color.blue);
            Debug.DrawLine(eyePos, eyePos + transform.right * turnSpeed * 2, Color.red);
            Debug.DrawLine(eyePos, eyePos + forward * speedAbs * 2, Color.yellow);
        }
    }

    private IEnumerator BeforeSlide()
    {
        currentState = PenguinState.BeforeSlide;
        beforeSlidePower = 0;
        rid.drag = beforeSlideDrag;
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

    private IEnumerator Slide()
    {
        currentState = PenguinState.Slide;
        anim.SetBool("Slide", true);
        anim.applyRootMotion = false;
        rid.drag = slideDrag;
        rid.AddForce(transform.forward * (slideForce + Mathf.Log(beforeSlidePower, 2)), ForceMode.Impulse);

        while (true)
        {
            yield return 0;
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
            virtualCamera.m_XAxis.m_InputAxisValue = x - turnSpeed * viewTurnDragFactor;
            virtualCamera.m_YAxis.m_InputAxisValue = y;
        }
    }

    public HarmSystem.HarmInformation GetHarmInformation(Collision collision)
    {
        return new HarmSystem.HarmInformation();
    }
}
