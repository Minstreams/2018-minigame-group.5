using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using GameSystem.PresentSetting;

namespace GameSystem
{
    /// <summary>
    /// 伤害系统
    /// </summary>
    public class HarmSystem : SubSystem<HarmSystemSetting>
    {
        /// <summary>
        /// 伤害计算结构
        /// </summary>
        public class HarmInformation
        {
            public float force;
            public float harm;
            public float destroyPower;
            public Vector3 direction;

            public HarmInformation(float force, float harm, float destroyPower, Vector3 direction)
            {
                this.force = force;
                this.harm = harm;
                this.destroyPower = destroyPower;
                this.direction = direction;
            }
            public HarmInformation() { }
        }

        /// <summary>
        /// 在天上飞的武器，用来计算碰撞伤害
        /// </summary>
        public interface FlyingAmmo
        {
            HarmInformation GetHarmInformation(Collision collision);
        }

        /// <summary>
        /// 被击中的目标物体
        /// </summary>
        public class HitTarget : NetworkBehaviour
        {
            private void OnCollisionEnter(Collision collision)
            {
                FlyingAmmo ammo = collision.collider.GetComponent<FlyingAmmo>();
                if (ammo != null)
                {
                    OnHarm(ammo.GetHarmInformation(collision));
                }
            }

            protected virtual void OnHarm(HarmInformation information)
            {
                Debug.Log(this.name + " is hitted. [force:" + information.force + "][harm:" + information.harm + "][destroyPower:" + information.destroyPower + "][direction:" + information.direction + "]");
            }
        }
    }
}
