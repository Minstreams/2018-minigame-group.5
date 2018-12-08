using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateChanger : MonoBehaviour {

	public void ChangeState(int state)
    {
        int count = transform.childCount;

        for(int i = 0; i < count; i++)
        {
            if (i == state) transform.GetChild(i).gameObject.SetActive(true);
            else transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
