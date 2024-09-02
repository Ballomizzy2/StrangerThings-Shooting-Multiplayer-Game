using UnityEngine;

[System.Serializable]
public class Weapon
{
	public string name = "Gun";
	public WeaponType weaponType;

	public int damage = 10;
	public float range = 100f;
	public float fireRate = 0f;

	public int maxBullets = 100;
	public int bullets;
	public int maxAmmo = 20;
	public int currenAmmo;

	public float reloadTime = 1f;
	public GameObject graphics;

	// camera
	public GameObject cam;
	// player mask
	public LayerMask mask;


	private const string ENEMY_TAG = "Enemy";

	public Weapon(string _name, WeaponType _weaponType,int _damage, float _range, float _fireRate, int _maxBullets, int _maxAmmo, float _reloadTime, GameObject _graphics, GameObject _cam, LayerMask _mask)
	{
		name = _name;
		weaponType = _weaponType;
		damage = _damage;
		range = _range;
		fireRate = _fireRate;
		reloadTime = _reloadTime;
		graphics = _graphics;
		
		maxBullets = _maxBullets;
		bullets = _maxBullets;

		maxAmmo = _maxAmmo;
		currenAmmo = _maxAmmo;

		cam = _cam;
		mask = _mask;

	}

	
	public void Reload()
    {
		// cannot reload when no ammo or just reloaded/ full bullets
		if (currenAmmo <= 0 || bullets == maxBullets)
			return;
		
		// animate

		// reload
		bullets = maxBullets;
		currenAmmo--;
		Debug.Log("Reloaded");

    }
}

[System.Serializable]
public enum WeaponType
{
    Pistol,
    AssaultRifle,
    Shotgun,
    Sniper,
    Rpg
}

public struct WeaponAttackData
{
	public bool attackedSomething;
	public GameObject objectAttacked;
}
