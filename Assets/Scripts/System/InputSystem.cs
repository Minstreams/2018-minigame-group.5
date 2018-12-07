using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem.PresentSetting;

namespace GameSystem
{
    /// <summary>
    /// 封装输入的系统
    /// </summary>
    public class InputSystem : SubSystem<InputSystemSetting>
    {


        public static bool GetInput(InputCode code)
        {
            switch (code)
            {
                case InputCode.Brake:
                    return Input.GetKey(Setting.brakeKeyCode);
                case InputCode.Slide:
                    return Input.GetKey(Setting.slideKeyCode);
                case InputCode.Func:
                    return Input.GetKey(Setting.funcKeyCode);
                case InputCode.Drop:
                    return Input.GetKeyDown(Setting.dropKeyCode);
                default:
                    return false;
            }
        }

        public static float GetHorizontalAxis()
        {
            return Input.GetAxis("Horizontal");
        }

        public static float GetVeticalAxis()
        {
            return Input.GetAxis("Vertical");
        }
    }

    /// <summary>
    /// enum of all possible input
    /// </summary>
    public enum InputCode
    {
        Brake,
        Slide,
        Func,
        Drop
    }
}
