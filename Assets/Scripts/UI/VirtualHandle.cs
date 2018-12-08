using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameSystem;
using UnityEngine.EventSystems;

public class VirtualHandle : ScrollRect
{
    private float radius = 0;
    public float scale = 0.9f;

    private VirtualLeft virtualLeft;
    public float outsideRadius = 1.2f;
    public Image outsideImage;
    public Image insideImage;
    public Image toggerImage;

    public Color outsideGray;

    protected override void Awake()
    {
        base.Awake();
        radius = viewport.sizeDelta.x / 2;
        virtualLeft = GetComponentInParent<VirtualLeft>();
    }

    private bool slide = false;
    public void Init()
    {
        slide = false;
        virtualLeft.ChangeColor(outsideImage, outsideGray, 0.2f);
        virtualLeft.ChangeColor(insideImage, Color.white, 0.2f);
        virtualLeft.ChangeColor(toggerImage, Color.white, 0.2f);
    }
    private void StartSlide()
    {
        slide = true;
        InputSystem.slide = true;
        virtualLeft.ChangeColor(outsideImage, Color.white, 0.2f);
        virtualLeft.ChangeColor(insideImage, Color.clear, 0.2f);
        virtualLeft.ChangeColor(toggerImage, outsideGray, 0.2f);
    }


    public override void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnDrag(eventData);

        Vector2 pos = eventData.position - (Vector2)transform.position;
        if (pos.magnitude > outsideRadius * radius)
        {
            StartSlide();
        }

        pos *= 0.9f;
        if (pos.magnitude > radius)
        {
            pos = pos.normalized * radius;
        }
        SetContentAnchoredPosition(pos);

        InputSystem.horizontal = pos.x / radius / 0.9f;
        InputSystem.vertical = pos.y / radius / 0.9f;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (slide)
        {
            InputSystem.slide = false;
            virtualLeft.ChangeColor(outsideImage, Color.clear, 0.2f);
            virtualLeft.ChangeToSlide();
        }
        InputSystem.horizontal = 0;
        InputSystem.vertical = 0;
    }




}
