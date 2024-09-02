using UnityEngine;

[System.Serializable]
public class Health
{
    public float maxHealth { private set; get; }
    public float currentHealth { private set; get; }
    public HealthType healthType { private set; get; }

    public Health(float _maxHealth, HealthType _healthType)
    {
        maxHealth = _maxHealth;
        currentHealth = _maxHealth;
        healthType = _healthType;
    }

    public void TakeDamage(float _amount)
    {
        currentHealth -= _amount + Random.Range(0, 3);
    }

    public void BoostHealth(float _amount, bool _fillUp = false)
    {
        // can only boost player health
        if (healthType != HealthType.Player)
            return;
        if (_fillUp)
        {
            currentHealth = maxHealth;
            return;
        }
        currentHealth += _amount;
    }

    public virtual void Die()
    {
        //Die
        Debug.Log("I died");
    }
}

public enum HealthType
{
    Player, Enemy, Object
}
