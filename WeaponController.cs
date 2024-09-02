using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Weapon weapon;
    [SerializeField]
    private WeaponStats weaponStats;

    Inventory inventory;
    WeaponManager weaponManager;
    LayerMask layerMask;
    Camera cam;

    private bool isReloading;
    public bool isPickable = true;
    private bool isWeaponClassSet = false;

    [SerializeField]
    private Transform firepoint;


    private void Start()
    {

        weapon = null;
        //inventory = GetComponentInParent<Inventory>();
        //weaponManager = inventory.weaponManager;

        ///cam = weaponManager.playerCam;
        //layerMask = weaponManager.layerMask;
        
        // set weapon details based on the data
        // without cam and layer mask details
        weapon = new Weapon
                (
                    weaponStats.Name, weaponStats.weaponType, weaponStats.damage, 
                    weaponStats.range, weaponStats.fireRate, weaponStats.maxBullets, 
                    weaponStats.maxAmmo, weaponStats.reloadTime, gameObject, 
                    null, 0
                );
        Debug.Log("Initialized Wepon");
        if (isPickable)
        {
            gameObject.AddComponent<Pickable>();
            transform.localScale = new Vector3(0.183880076f, 0.174390003f, 0.174390063f);
        }
        else
            Destroy(GetComponent<Outline>());


    }

    private void Update()
    {
        if (weapon != null && !isWeaponClassSet)
        {
            if (!isPickable)
            {
                gameObject.SetActive(false);
                isWeaponClassSet = true;
            }
        }
    }

    public void Shoot()
    {
        Bullet newBullet;

        //newBullet = Instantiate(weaponStats.bullet, firepoint.transform.position, Quaternion.identity).GetComponent<Bullet>();
        //newBullet.transform.SetParent(transform);
        //newBullet.transform.forward = weapon.cam.transform.forward;
        Ray cameraRay = new Ray(weapon.cam.transform.position, weapon.cam.transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(cameraRay, out hit))
        {
            Vector3 target = hit.point;
            Vector3 direction = (target - firepoint.transform.position);//.normalized;
            newBullet = SpawnManager.SpawnANetworkObject(weaponStats.bullet.name, firepoint.transform.position, Quaternion.LookRotation(direction)).GetComponent<Bullet>();
            //newBullet.transform.forward = direction;
            newBullet.MoveBullet(weapon.range, weapon.damage);
        }

        weapon.bullets--;

        // play sound
        switch (weapon.weaponType)
        {
            case WeaponType.Rpg:
                //sound
                break;
            case WeaponType.Sniper:
                //sound
                break;
            case WeaponType.Shotgun:
                //sound
                break;
            case WeaponType.AssaultRifle:
                //sound
                break;
            case WeaponType.Pistol:
                //sound
                break;
            default:
                break;
        }
        
    }

    public void Reload()
    {
        if (isReloading)
            return;
        if (weapon.currenAmmo > 0 && weapon.bullets != weapon.maxBullets)
        {
            Debug.Log("Reloaded");
            // wait for relaod time
            StartCoroutine(ReloadAnimation());
            weapon.bullets = weapon.maxBullets;
            weapon.currenAmmo--;
        }
        else
            Debug.Log("Out of Ammo!");
    }

    private IEnumerator ReloadAnimation()
    {
        isReloading = true;
        // animate here
        yield return new WaitForSeconds(weapon.reloadTime);
        isReloading = false;

    }

    public void SetNewWeaponData(Weapon _weaponStats, GameObject _cam, LayerMask _layerMask)
    {
        weapon = new Weapon
        (
            gameObject.name, _weaponStats.weaponType, _weaponStats.damage, 
            _weaponStats.range, _weaponStats.fireRate, _weaponStats.maxBullets,
            _weaponStats.maxAmmo, _weaponStats.reloadTime, gameObject, 
            _cam.gameObject, _layerMask
        );
        isPickable = false;
    }

    public void Drop(Vector3 spot, Vector3 scale)
    {
        isPickable = true;
        transform.parent = null;
        transform.position = spot;
        transform.localScale = scale;
        gameObject.SetActive(true);
    }

   


}
