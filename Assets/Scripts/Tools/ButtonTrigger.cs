using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("自制工具/Button Trigger")]
public class ButtonTrigger : MonoBehaviour
{
    private Button button;
    public GameSystem.GameMessage message;
    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => { GameSystem.TheMatrix.SendGameMessage(message); });
    }
}
