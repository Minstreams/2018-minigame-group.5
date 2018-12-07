using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    namespace PresentSetting
    {
        [CreateAssetMenu(fileName = "ItemSystemSetting", menuName = "系统配置文件/Item System Setting")]
        public class ItemSystemSetting : ScriptableObject
        {
            public float pickRange = 1;
            public List<Item> itemList = new List<Item>();
            public float itemDropSpeed = 8;
        }
    }
}
