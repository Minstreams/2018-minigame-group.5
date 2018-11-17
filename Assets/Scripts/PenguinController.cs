using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;
using UnityEngine.Networking;


/// <summary>
/// 企鹅控制器
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PenguinController : NetworkBehaviour
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

    private void Awake()
    {
        rid = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        Cinemachine.CinemachineVirtualCamera cam = GameObject.Instantiate(camPrefab).GetComponent<Cinemachine.CinemachineVirtualCamera>();
        cam.Follow = transform;
        cam.LookAt = transform.GetChild(2);
        StartCoroutine(Walk());
        StartCoroutine(Func());
    }


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
            Vector3 forward = (v * camForward - h * Vector3.Cross(camForward, Vector3.up)).normalized;

            float angle = Vector3.SignedAngle(transform.forward, forward, Vector3.up);  //前进转角

            float speedAbs = Mathf.Max(Mathf.Abs(v), Mathf.Abs(h));
            float currentSpeed = Vector3.Dot(transform.forward, forward) * walkSpeed * speedAbs;
            float turn = Vector3.Dot(transform.right, forward) * walkSpeed * speedAbs;

            //播放转身动画
            anim.SetFloat("turn", turn);

            //TODO:可以试图改为Torque
            //transform.Rotate(Vector3.up, angle * (1 - Mathf.Pow(1 - rotateRate, Time.timeScale)));

            //播放行走动画
            anim.SetFloat("speed", currentSpeed);
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
}
