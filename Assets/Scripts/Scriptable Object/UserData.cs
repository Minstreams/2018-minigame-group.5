using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "User Data", menuName = "User Data")]
public class UserData : SavableObject {
    public int imageIndex;
    public Color headColor;
    public Color bodyColor;
    public string userName;
    public int musicVolume;
    public int soundVolume;
}
