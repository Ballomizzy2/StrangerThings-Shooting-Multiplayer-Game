using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Detail", menuName = "Enemy Types", order = 1)]
public class EnemyData : ScriptableObject
{
    public string Name;
    [Range(1f, 100f)]
    public float speed;
    [Range(10f, 500f)]
    public float damage;
    [Range(0f, 1f)]
    public float intelligence;
    [Range(-5f, 20f)]
    public float attackDistance = 0f;
    [Range(100f, 1000f)]
    public float maxHealth;

    public EnemyType enemyType;
}
