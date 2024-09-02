using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private WeaponType weaponTypeBullet;

    [SerializeField]
    [Range(120f, 300f)]
    private float bulletSpeed;
    [SerializeField]
    private float bulletTime;

    private float damage;

    private bool rpgBullet;
    private Rigidbody rb;
    private SpawnManager spawnManager;
    
    private void Awake()
    {
        spawnManager = FindObjectOfType<SpawnManager>();
        //MoveBullet();
        if(weaponTypeBullet == WeaponType.Rpg)
        {
            rb = GetComponent<Rigidbody>();
            transform.localScale = Vector3.one * 2.5f;
        }
    }

    public void MoveBullet(float _distance, float _damage)
    {
        //transform.SetParent(null);
        bulletTime = _distance / bulletSpeed;
        damage = _damage;

        //Destroy(gameObject, bulletTime);
        spawnManager.DestroyNetworkObjectInSeconds(gameObject, bulletTime);
        switch (weaponTypeBullet)
        {
            case WeaponType.Rpg:
                RpgShoot();
                break;
            default:
                NormalShoot();
                break;
        }
    }

    private void Update()
    {
        if (rpgBullet)
        {
            rb.useGravity = true;
            rb.AddForce((transform.forward + (Vector3.up * .02f)) * bulletSpeed, ForceMode.Force);

        }

        else
            transform.position += transform.forward * bulletSpeed * Time.deltaTime;
        
    }
    public void RpgShoot()
    {
        rpgBullet = true;
    }

    public void NormalShoot()
    {
        rpgBullet = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //touchedSomething = true;
        if(other.gameObject.CompareTag("Enemy"))
        {
            //blah blah
            other.gameObject.GetComponent<EnemyController>().TakeDamage(damage);
            //Destroy(gameObject);    
            SpawnManager.Destroy(gameObject);
        }
        Debug.Log("Successfully Hit som " + other.gameObject.name);
        SpawnManager.Destroy(gameObject);
        //Destroy(gameObject);
        //if(!other.gameObject.CompareTag("Weapon"))
    }
}
