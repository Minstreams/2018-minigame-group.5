using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameSystem;

public class VirtualLeft : MonoBehaviour
{
    public VirtualHandle walkControl;
    public GameObject slideControl;
    public Image brakeImage;


    private void Start()
    {
        ChangeToSlide();
    }

    public void ChangeToSlide()
    {
        StartCoroutine(_ChangeToSlide());
    }

    private IEnumerator _ChangeToSlide()
    {
        yield return new WaitForSeconds(0.2f);
        walkControl.gameObject.SetActive(false);
        slideControl.SetActive(true);
        yield return _ChangeColor(brakeImage, Color.white, 0.2f);
        while (LevelSystem.LocalPlayer.CurrentState != PenguinController.PenguinState.Walk) yield return 0;
        yield return _ChangeColor(brakeImage, Color.clear, 0.2f);
        walkControl.gameObject.SetActive(true);
        slideControl.SetActive(false);
        walkControl.Init();
    }

    public void ChangeColor(Image image, Color color, float time)
    {
        StartCoroutine(_ChangeColor(image, color, time));
    }
    private IEnumerator _ChangeColor(Image image, Color color, float time)
    {
        Color c = image.color;
        float t = 0;
        while (t < 1)
        {
            yield return 0;
            t += Time.deltaTime / time;
            image.color = Color.Lerp(c, color, t);
        }
        yield return 0;
        image.color = color;
    }

    public void SetBrake(bool b)
    {
        InputSystem.brake = b;
    }

    public void SetFunc(bool b)
    {
        InputSystem.func = b;
    }

    public void SetDrop()
    {
        InputSystem.drop = true;
    }
}
