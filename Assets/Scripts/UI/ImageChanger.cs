using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameSystem;

/// <summary>
/// 换头像器
/// </summary>
public class ImageChanger : MonoBehaviour
{
    public Image image;
    public Image headImage;
    public Image headImage2;
    public Image bodyImage;
    public Image bodyImage2;

    public InputField nameField;

    private void Start()
    {
        image.sprite = PublicDateSystem.Data.headImages[PublicDateSystem.Data.userData.imageIndex];
        headImage2.color = headImage.color = PublicDateSystem.Data.userData.headColor;
        bodyImage2.color = bodyImage.color = PublicDateSystem.Data.userData.bodyColor;
        nameField.text = PublicDateSystem.Data.userData.userName;
    }

    public void ChangeImage()
    {
        image.sprite = PublicDateSystem.SetRandomHeadImage();
    }

    public void ChangeHeadColor()
    {
        headImage2.color = headImage.color = PublicDateSystem.SetRandomHeadColor();
    }

    public void ChangeBodyColor()
    {
        bodyImage2.color = bodyImage.color = PublicDateSystem.SetRandomBodyColor();
    }

    public void SaveName(string n)
    {
        PublicDateSystem.Data.userData.userName = n;
        PublicDateSystem.Data.userData.saved = false;
    }
}
