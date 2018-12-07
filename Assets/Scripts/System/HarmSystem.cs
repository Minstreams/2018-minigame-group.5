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
        public static float HarmFactor { get { return Setting.harmFactor; } }


        /// <summary>
        /// 伤害计算结构
        /// </summary>
        public class HarmInformation
        {
            public float force;
            public float harm;
            public float destroyPower;
            public Vector3 direction;
            public Vector3 position;

            public HarmInformation(float force, float harm, float destroyPower, Vector3 direction, Vector3 position)
            {
                this.force = force;
                this.harm = harm;
                this.destroyPower = destroyPower;
                this.direction = direction;
                this.position = position;
            }
            public HarmInformation() { }
        }

        /// <summary>
        /// 被击中的目标物体
        /// </summary>
        public class HitTarget : NetworkBehaviour
        {
            public virtual void OnServerHarm(HarmInformation information)
            {
                Debug.Log(this.name + " is hitted. [force:" + information.force + "][harm:" + information.harm + "][destroyPower:" + information.destroyPower + "][direction:" + information.direction + "]");
            }
        }


        public static void ShowFloatingNumber(string text, Vector3 position)
        {
            TextMesh tm = GameObject.Instantiate(Setting.harmFloatingNumber, position, Quaternion.LookRotation(Camera.main.transform.position - position), null).GetComponent<TextMesh>();
            tm.text = text;
        }
    }
}
