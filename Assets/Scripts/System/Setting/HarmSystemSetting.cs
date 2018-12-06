using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameSystem
{
    namespace PresentSetting
    {
        [CreateAssetMenu(fileName = "HarmSystemSetting", menuName = "系统配置文件/Harm System Setting")]
        public class HarmSystemSetting : ScriptableObject
        {
            public float harmFactor = 0.01f;
            public GameObject harmFloatingNumber;
        }
    }
}
