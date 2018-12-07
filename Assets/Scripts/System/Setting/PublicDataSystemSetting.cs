using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace PresentSetting
    {
        [CreateAssetMenu(fileName = "PublicDataSystemSetting", menuName = "系统配置文件/PublicData System Setting")]
        public class PublicDataSystemSetting : ScriptableObject
        {
            public AudioClip pickUp;
            public AudioClip runOutOfAmmo;
        }
    }
}
