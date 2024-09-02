public class Enemy
{
    public float speed,
                 damage,
                 intelligence,
                 attackDistance,
                 maxHealth;
       

    public EnemyType enemyType;

    public Enemy(float speed, float damage, float intelligence, float attackDistance, float maxHealth,EnemyType enemyType)
    {
        this.speed = speed;
        this.damage = damage;
        this.intelligence = intelligence;
        this.attackDistance = attackDistance;
        this.maxHealth = maxHealth;
        this.enemyType = enemyType;
    }
}
[System.Serializable]
public enum EnemyType
{
    Demodogs, Demobats, Demogorgon, Mindflayer, Vecna
}
