using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VirtualInput : MonoBehaviour {
	public static float horizontal;
	public static float vertical;
	public static bool slide;

	private RectTransform tran;
	private float range;

	// Use this for initialization
	void Start () {
		tran = GetComponent<RectTransform>();
		range = transform.parent.GetComponent<RectTransform> ().sizeDelta.x / 2f;
	}
	
	// Update is called once per frame
	void Update () {
		horizontal = tran.anchoredPosition.x / range;
		vertical = tran.anchoredPosition.y / range;
		//print(horizontal);
	}
}
