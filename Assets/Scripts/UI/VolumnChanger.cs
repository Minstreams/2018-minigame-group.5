using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using GameSystem;

public class VolumnChanger : MonoBehaviour
{
    public AudioMixer mixer;
    public bool isAlone;
    public StateChanger soundChanger0;
    public StateChanger soundChanger1;
    public StateChanger soundChanger2;
    public StateChanger musicChanger0;
    public StateChanger musicChanger1;
    public StateChanger musicChanger2;


    private void Start()
    {
        if (!isAlone) return;
        mixer.SetFloat("SoundV", PublicDateSystem.Data.userData.soundVolume * 10 - 20);
        mixer.SetFloat("MusicV", PublicDateSystem.Data.userData.musicVolume * 10 - 20);
    }

    private void OnEnable()
    {
        if (isAlone) return;
        int v = PublicDateSystem.Data.userData.soundVolume;
        soundChanger0.ChangeState(v == 0 ? 0 : 1);
        soundChanger1.ChangeState(v);
        soundChanger2.ChangeState(v < 2 ? 0 : 1);
        v = PublicDateSystem.Data.userData.musicVolume;
        musicChanger0.ChangeState(v == 0 ? 0 : 1);
        musicChanger1.ChangeState(v);
        musicChanger2.ChangeState(v < 2 ? 0 : 1);
    }
    public void ChangeSound(int v)
    {
        PublicDateSystem.Data.userData.soundVolume = v;
        PublicDateSystem.Data.userData.saved = false;
        mixer.SetFloat("SoundV", v * 10 - 20);
    }
    public void ChangeMusic(int v)
    {
        PublicDateSystem.Data.userData.musicVolume = v;
        PublicDateSystem.Data.userData.saved = false;
        mixer.SetFloat("MusicV", v * 10 - 20);
    }
}
