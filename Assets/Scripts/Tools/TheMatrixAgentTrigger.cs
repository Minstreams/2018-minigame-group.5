using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameSystem;

/// <summary>
/// 母体代理触发器，用于给The Matrix发送控制信息
/// </summary>
[AddComponentMenu("自制工具/母体代理触发器")]
public class TheMatrixAgentTrigger : MonoBehaviour
{
    [Header("母体代理触发器，使用键盘输入给The Matrix发送控制信息")]
    public KeyCode keyCode = KeyCode.F;
    public GameMessage messageToSend;

    private void Update()
    {
        if (Input.GetKeyDown(keyCode))
        {
            TheMatrix.SendGameMessage(messageToSend);
            print(messageToSend + " trigged!");
        }
    }
}
