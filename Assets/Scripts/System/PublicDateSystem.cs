using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem.PresentSetting;

namespace GameSystem
{
    /// <summary>
    /// 一些公用数据
    /// </summary>
    public class PublicDateSystem : SubSystem<PublicDataSystemSetting>
    {
        public static PublicDataSystemSetting Data { get { return Setting; } }

    }
}
