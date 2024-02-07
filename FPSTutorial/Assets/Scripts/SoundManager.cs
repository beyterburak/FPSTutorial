using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource ShootingChannel;
    
    public AudioClip M1911Shot;
    public AudioClip M4Shot;

    public AudioSource reloadingSoundM4;
    public AudioSource reloadingSoundM1911;

    public AudioSource emptyMagazineSoundM1911;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;    
        }
    }

    public void PlayShootingSound(Weapon.WeaponModel weapon)
    {
        switch (weapon)
        {
            case Weapon.WeaponModel.M1911:
                ShootingChannel.PlayOneShot(M1911Shot); 
                break;
            case Weapon.WeaponModel.M4:
                ShootingChannel.PlayOneShot(M4Shot);
                break;
        }
    }

    public void PlayReloadSound(Weapon.WeaponModel weapon)
    {
        switch (weapon)
        {
            case Weapon.WeaponModel.M1911:
                reloadingSoundM1911.Play();
                break;
            case Weapon.WeaponModel.M4:
                reloadingSoundM4.Play();
                break;
        }
    }
}
