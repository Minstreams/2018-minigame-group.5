using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VirtualHandle : ScrollRect {

	private float radius = 0;

	// Use this for initialization
	void Start () {
		base.Start ();
		radius = (viewport.transform as RectTransform).sizeDelta.x * 0.5f;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public override void OnDrag (UnityEngine.EventSystems.PointerEventData eventData)
	{
		base.OnDrag (eventData);
		Vector2 pos = content.anchoredPosition;
		if (pos.magnitude > radius) {
			pos = pos.normalized * radius;
			SetContentAnchoredPosition (pos);
		}
	}
}
