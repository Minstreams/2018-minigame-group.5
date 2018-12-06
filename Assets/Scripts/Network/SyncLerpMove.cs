using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkIdentity))]
public class SyncLerpMove : NetworkBehaviour
{
    /// <summary>
    /// 一秒内同步的次数
    /// </summary>
    public float NetworkSendRate = 9;
    private float nextSyncTime;

    public bool sync = true;
    public bool debug = false;
    public Transform targetTransform;
    public Rigidbody targetRigidbody;
    public bool syncRigidbody;

    /*同步3D坐标*/
    public bool SyncTransform3D = true;
    private Vector3 syncPosition3D;
    private Vector3 syncVelocity;
    private Vector3 syncAngleVec;
    /*同步转向*/
    public bool SyncRotation3D = true;
    private Quaternion syncRotation3D;

    [Range(0, 1)]
    public float lerpRate = 0.01f;

    public void SetSyncVar()
    {
        if (syncRigidbody)
        {
            if (SyncTransform3D) syncPosition3D = targetRigidbody.position;
            if (SyncRotation3D) syncRotation3D = targetRigidbody.rotation;
            syncVelocity = targetRigidbody.velocity;
            syncAngleVec = targetRigidbody.angularVelocity;
        }
        else
        {
            if (SyncTransform3D) syncPosition3D = targetTransform.position;
            if (SyncRotation3D) syncRotation3D = targetTransform.rotation;
        }
    }
    public void SetSyncVar(Vector3 position, Quaternion rotation)
    {
        syncPosition3D = position;
        syncRotation3D = rotation;
    }

    [ClientRpc]
    private void RpcSendSyncVar(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angleVec)
    {
        recordCounter++;
        syncPosition3D = position;
        syncRotation3D = rotation;
        syncVelocity = velocity;
        syncAngleVec = angleVec;
    }

    public void ApplySyncVar()
    {
        if (syncRigidbody)
        {
            if (SyncTransform3D) targetRigidbody.position = syncPosition3D;
            if (SyncRotation3D) targetRigidbody.rotation = syncRotation3D;
            targetRigidbody.velocity = syncVelocity;
            targetRigidbody.angularVelocity = syncAngleVec;
        }
        else
        {
            if (SyncTransform3D) targetTransform.position = syncPosition3D;
            if (SyncRotation3D) targetTransform.rotation = syncRotation3D;
        }
    }

    public void ApplySyncVarInterpolated()
    {
        if (syncRigidbody)
        {
            if (SyncTransform3D)
            {
                Quaternion deltaRot = syncRotation3D * Quaternion.Inverse(targetRigidbody.rotation);
                Vector3 delta = (syncPosition3D - targetRigidbody.position);
                Quaternion vecRot = Quaternion.FromToRotation(targetRigidbody.velocity, syncVelocity);
                Vector3 vecDelta = syncVelocity - vecRot * targetRigidbody.velocity;
                Vector3 angleVecDelta = syncAngleVec - targetRigidbody.angularVelocity;
                foreach (Rigidbody r in rigidBodies)
                {
                    Vector3 zhou = r.position - targetRigidbody.position;
                    Vector3 offset = deltaRot * zhou - zhou;
                    r.MovePosition(r.position + lerpRate * delta + lerpRate * offset);
                    r.MoveRotation(Quaternion.Slerp(Quaternion.identity, deltaRot, lerpRate) * r.rotation);
                    r.velocity = vecRot * r.velocity + lerpRate * vecDelta;
                    r.AddTorque(lerpRate * angleVecDelta, ForceMode.VelocityChange);
                }
            }
            //if (SyncRotation3D) targetRigidbody.MoveRotation(syncRotation3D);
        }
        else
        {
            if (SyncTransform3D) targetTransform.position = Vector3.Lerp(targetTransform.position, syncPosition3D, lerpRate);
            if (SyncRotation3D) targetTransform.rotation = Quaternion.Slerp(targetTransform.rotation, syncRotation3D, lerpRate);
        }
    }

    private Rigidbody[] rigidBodies;
    private void Awake()
    {
        rigidBodies = GetComponentsInChildren<Rigidbody>();
    }
    private void Start()
    {
        SetSyncVar();
    }


    float recordTimer = 1;
    int recordCounter = 0;
    public int realSendRate = 0;
    void FixedUpdate()
    {
        if (!sync) return;

        recordTimer -= Time.fixedDeltaTime;
        if (recordTimer <= 0)
        {
            recordTimer += 1;
            realSendRate = recordCounter;
            recordCounter = 0;
        }

        if (GameSystem.NetworkSystem.IsServer)
        {
            nextSyncTime -= Time.fixedDeltaTime;
            if (nextSyncTime > 0) return;
            nextSyncTime = 1f / NetworkSendRate;

            SetSyncVar();
            RpcSendSyncVar(syncPosition3D, syncRotation3D, syncVelocity, syncAngleVec);
        }
        else
        {
            ApplySyncVarInterpolated();
        }
    }
}
