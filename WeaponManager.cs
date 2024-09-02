using UnityEngine;
using UnityEngine.UI;
using StarterAssets;
using Photon.Pun;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    private WeaponController currentWeapon;

    [Header("Gun Types")]
    [SerializeField]
    private GameObject currentWeaponGO;
    
    // all type of guns game objects
    [SerializeField]
    private GameObject rpg, pistol, sniper, shotgun, assaultRifle, regular;

    [Space]
    [Header("Cross Hair")]
    // cross hair image reference
    [SerializeField]
    private Image crossHairImage;
    // all type of guns cross hairs
    [SerializeField]
    private Sprite rpgCrossHair, pistolCrossHair, sniperCrossHair, shotgunCrossHair, assaultRifleCrossHair, regularCrossHair;

    [Space]
    [Header("Shooting and Input")]
    // input
    [SerializeField]
    private StarterAssetsInputs _input;

    private PhotonView view;


    // shooting
    public Camera playerCam;
    public LayerMask layerMask;

    // Camera Shake for guns
    [SerializeField]
    private PlayerController playerController;
    private PlayerCameraShake cam, aimCam;


    private void Awake()
    {
        view = GetComponent<PhotonView>();
        cam = playerController.GetCameraPlayerShake();
        aimCam = playerController.GetAimCameraPlayerShake();
        //_input = transform.parent.GetComponentInChildren<PlayerInput>();
        //playerCam = transform.parent.GetComponent<Camera>();
        //layerMask = ~layerMask;
    }

    private void Start()
    {
        if (!_input)
            _input = playerController.GetInput();
        
    }

    public StarterAssets.StarterAssetsInputs GetInput()
    {
        return _input;
    }

    public GameObject SwitchWeapon(WeaponController weaponController)
    {
        // switch cross hair  
        if (MyPlayer())
        {
            switch (weaponController.weapon.weaponType)
            {
                case WeaponType.Sniper:
                    crossHairImage.sprite = sniperCrossHair;
                    currentWeaponGO = sniper;
                    break;
                case WeaponType.Shotgun:
                    crossHairImage.sprite = shotgunCrossHair;
                    currentWeaponGO = shotgun;
                    break;
                case WeaponType.Rpg:
                    crossHairImage.sprite = rpgCrossHair;
                    currentWeaponGO = rpg;
                    break;
                case WeaponType.AssaultRifle:
                    crossHairImage.sprite = assaultRifleCrossHair;
                    currentWeaponGO = assaultRifle;
                    break;
                case WeaponType.Pistol:
                    crossHairImage.sprite = pistolCrossHair;
                    currentWeaponGO = pistol;
                    break;
                default:
                    crossHairImage.sprite = regularCrossHair;
                    currentWeaponGO = regular;
                    break;
            }
        }
        currentWeapon = weaponController;

        return DrawWeapon(currentWeapon.weapon);
    }

    private Weapon weaponToDraw;
    private GameObject gameObjectDrawn;
    public GameObject DrawWeapon(Weapon _weapon)
    {

        weaponToDraw = _weapon;
        //view.RPC("RPC_DrawWeapon", RpcTarget.All);
        RPC_DrawWeapon();
        return gameObjectDrawn;
    }
    [PunRPC]
    private void RPC_DrawWeapon()
    {
        Transform t, newWeapon = null;
        // set new weapon active
        for (int i = 0; i < transform.childCount; i++)
        {
            t = transform.GetChild(i);
            WeaponController controller = t.gameObject.GetComponent<WeaponController>();
            if (controller.weapon.weaponType == weaponToDraw.weaponType)
            {
                //t.gameObject.SetActive(true);
                newWeapon = t;
            }
            else
                t.gameObject.SetActive(false);
        }
        // put the weapon data of the previous weapon in the drawn weapon
        newWeapon.GetComponent<WeaponController>().SetNewWeaponData(weaponToDraw, playerCam.gameObject, layerMask);
        gameObjectDrawn = newWeapon.gameObject;
    }

    public void Update()
    {
        Shoot();
        Reload();
    }

    public void Shoot()
    {
        if (MyPlayer())
        {
            if (currentWeapon && _input.shoot && !_input.sprint)
            {
                if(currentWeapon.weapon.bullets <= 0)
                {
                    return;
                }
                if (currentWeapon.weapon.fireRate <= 0f)
                {
                    Debug.Log("Attempt Shoot");
                    currentWeapon.Shoot();
                    aimCam.Shake(.1f, .1f, true);
                    cam.Shake(.1f, .1f, true);
                }
                else
                {
                    Debug.Log("Attempt Shoot");
                    StartCoroutine(ShootMethodRef());
                }
            }
        }
    }

    public void Reload()
    {
        if (MyPlayer() && currentWeapon != null)
        {
            if (currentWeapon.weapon.bullets <= 0 || (Input.GetKeyDown(KeyCode.R) && currentWeapon.weapon.bullets < currentWeapon.weapon.maxBullets))
            {
                Debug.Log("Attempt Reload");
                currentWeapon.Reload();
            }
            else
                Debug.Log("Cannot Reload");
               

        }
    }

    public bool isHoldingWeapon()
    {
        return currentWeapon ? true : false;
    }
    
    public void RefillAmmo()
    {
        if(currentWeapon != null)
            currentWeapon.weapon.currenAmmo = currentWeapon.weapon.maxAmmo;
    }

    public IEnumerator ShootMethodRef()
    {
        currentWeapon.Shoot();
        aimCam.Shake(.1f, .1f, true);
        cam.Shake(.1f, .1f, true);
        yield return new WaitForSeconds(1f/currentWeapon.weapon.fireRate);
    }

    public bool MyPlayer()
    {
        return view.IsMine;
    }

}


[System.Serializable]
public struct WeaponData
{
    public GameObject weaponGameObject;
    public Weapon weaponClass;

    public WeaponData(GameObject _weaponGameObject, Weapon _weaponClass)
    {
        weaponClass = _weaponClass;
        weaponGameObject = _weaponGameObject;   
    }
}


