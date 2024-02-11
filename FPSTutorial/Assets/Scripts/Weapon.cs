using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class Weapon : MonoBehaviour
{
    public bool isActiveWeapon;

    [Header("Shooting")]
    //Shooting
    [SerializeField] private bool isShooting, readyToShoot;
    [SerializeField] private bool allowReset = true;
    [SerializeField] private float shootingDelay = 2f;

    [Header("Burst")]
    //Burst
    public int bulletsPerBurst = 3;
    [SerializeField] private int burstBulletsLeft;

    [Header("Spread")]
    //Spread
    [SerializeField] float spreadIntensity;
    public float hipSpreadIntensity;
    public float adsSpreadIntensity;

    [Header("Bullet")]
    //Bullet
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    [SerializeField] private float bulletVelocity = 30;
    [SerializeField] private float bulletPrefabLifeTime = 3f;
    
    public GameObject muzzleEffect;
    internal Animator animator;

    [Header("Loading")]
    //Loading
    [SerializeField] private float reloadTime;
    public int magazineSize, bulletsLeft;
    [SerializeField] private bool isReloading;

    public Vector3 spawnPosition;
    public Vector3 spawnRotation;

    public bool isADS;

    public enum WeaponModel
    {
        M1911,
        M4
    }

    public WeaponModel thisWeaponModel;

    public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }

    public ShootingMode currentShootingMode;

    private void Awake()
    {
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;
        animator = GetComponent<Animator>();

        bulletsLeft = magazineSize;

        spreadIntensity = hipSpreadIntensity;
    }

    void Update()
    {
        if (isActiveWeapon)
        {
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                EnterADS();
            }

            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                ExitADS();
            }

            GetComponent<Outline>().enabled = false;

            if (bulletsLeft == 0 && isShooting)
            {
                SoundManager.instance.emptyMagazineSoundM1911.Play();
            }

            if (currentShootingMode == ShootingMode.Auto)
            {
                //Holding Down Left Mouse Button
                isShooting = Input.GetKey(KeyCode.Mouse0);
            }
            else if (currentShootingMode == ShootingMode.Single || currentShootingMode == ShootingMode.Burst)
            {
                //Clicking Left Mouse Button
                isShooting = Input.GetKeyDown(KeyCode.Mouse0);
            }

            if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && isReloading == false && isShooting == false && WeaponManager.instance.CheckAmmoLeftFor(thisWeaponModel) > 0)
            {
                Reload();
            }

            //if you want to automatically reload when magazine is empty
            if (readyToShoot && isShooting == false && isReloading == false && bulletsLeft <= 0)
            {
                //Reload();
            }

            if (readyToShoot && isShooting && bulletsLeft > 0)
            {
                burstBulletsLeft = bulletsPerBurst;
                FireWeapon();
            }
        }
    }

    private void EnterADS()
    {
        animator.SetTrigger("enterADS");
        isADS = true;
        HUDManager.instance.middleDot.SetActive(false);
        spreadIntensity = adsSpreadIntensity;
    }

    private void ExitADS()
    {
        animator.SetTrigger("exitADS");
        isADS = false;
        HUDManager.instance.middleDot.SetActive(true);
        spreadIntensity = hipSpreadIntensity;
    }

    private void FireWeapon()
    {
        bulletsLeft--;

        muzzleEffect.GetComponent<ParticleSystem>().Play();

        if (isADS)
        {
            animator.SetTrigger("RECOIL_ADS");
        }
        else
        {
            animator.SetTrigger("RECOIL");
        }
        
        SoundManager.instance.PlayShootingSound(thisWeaponModel);

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;   

        //Instantiate the bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);

        //Pointing the bullet to face the shooting direction
        bullet.transform.forward = shootingDirection;

        //Shoot the bullet
        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);

        //Destroy the bullet after some time
        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));

        //Checking if we are done shooting
        if (allowReset)
        {
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }

        //Burst Mode
        if (currentShootingMode == ShootingMode.Burst && burstBulletsLeft > 1) //we already shoot once before this check
        {
            burstBulletsLeft--;
            Invoke("FireWeapon", shootingDelay);
        }
    }

    private void Reload()
    {
        //SoundManager.instance.reloadingSoundM1911.Play();
        SoundManager.instance.PlayReloadSound(thisWeaponModel);
        animator.SetTrigger("RELOAD");

        isReloading = true;
        readyToShoot = false;
        Invoke("ReloadComleted", reloadTime);
    }

    private void ReloadComleted()
    {
        if (WeaponManager.instance.CheckAmmoLeftFor(thisWeaponModel) > magazineSize)
        {
            bulletsLeft = magazineSize - bulletsLeft;
            WeaponManager.instance.DecreaseTotalAmmo(bulletsLeft, thisWeaponModel);
            bulletsLeft = magazineSize;
        }
        else
        {
            bulletsLeft = WeaponManager.instance.CheckAmmoLeftFor(thisWeaponModel);
            WeaponManager.instance.DecreaseTotalAmmo(bulletsLeft, thisWeaponModel);
        }

        
        isReloading = false;
        readyToShoot = true;    
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    public Vector3 CalculateDirectionAndSpread()
    {
        //Shooting from the middle of the screen to check where are we pointing at
        if (thisWeaponModel == WeaponModel.M4)
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out hit))
            {
                //Hitting Something
                targetPoint = hit.point;
            }
            else
            {
                //Shooting at the air
                targetPoint = ray.GetPoint(100);
            }
            Vector3 direction = targetPoint - bulletSpawn.position;

            float x = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
            float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

            //Returning the shooting direction and spread
            return direction + new Vector3(x, y, 0);
        }

        if (thisWeaponModel == WeaponModel.M1911)
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0f, 0.5f, 0));
            RaycastHit hit;

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out hit))
            {
                //Hitting Something
                targetPoint = hit.point;
            }
            else
            {
                //Shooting at the air
                targetPoint = ray.GetPoint(100);
            }
            Vector3 direction = targetPoint - bulletSpawn.position;

            float x = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
            float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

            //Returning the shooting direction and spread
            return direction + new Vector3(x, y, 0);
        }

        else
        {
            return new Vector3(0, 0, 0);
        }
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}
