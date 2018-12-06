using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStartMark : MonoBehaviour {
	void Start () {
        GameSystem.TheMatrix.SendGameMessage(GameSystem.GameMessage.Loaded);
	}
}
