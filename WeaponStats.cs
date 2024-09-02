using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon Name Detail", menuName = "Wepon Types", order = 1)]
public class WeaponStats : ScriptableObject
{
	public string Name;
	public WeaponType weaponType;

	public int damage = 10;
	public float range = 100f;
	public float fireRate = 0f;

	public int maxBullets = 100;
	public int maxAmmo = 20;

	public float reloadTime = 1f;
	public GameObject graphics;
	public GameObject bullet;
}
