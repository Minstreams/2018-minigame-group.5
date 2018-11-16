using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem
{
    /// <summary>
    /// 所有子系统的父类。
    /// </summary>
    /// <typeparam name="SubSetting">子系统设置类，必须继承自ScriptableObject</typeparam>
    public abstract class SubSystem<SubSetting> where SubSetting : ScriptableObject
    {
        private static SubSetting _Setting;
        protected static SubSetting Setting
        {
            get
            {
                if (_Setting == null)
                {
                    _Setting = Resources.Load<SubSetting>(typeof(SubSetting).ToString().Substring(26));
                }
                return _Setting;
            }
        }

        /// <summary>
        /// 基于母体协程管理工程实现的功能，开始协程
        /// </summary>
        public static LinkedListNode<Coroutine> StartCoroutine(IEnumerator routine)
        {
            return TheMatrix.StartCoroutine(routine, typeof(SubSetting));
        }
        /// <summary>
        /// 基于母体协程管理工程实现的功能，停止所有协程
        /// </summary>
        public static void StopAllCoroutines()
        {
            TheMatrix.StopAllCoroutines(typeof(SubSetting));
        }
        /// <summary>
        /// 基于母体协程管理工程实现的功能，停止协程
        /// </summary>
        public static void StopCoroutine(LinkedListNode<Coroutine> node)
        {
            TheMatrix.StopCoroutine(node);
        }
    }
}