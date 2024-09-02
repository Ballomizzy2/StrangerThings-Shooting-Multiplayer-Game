using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Wave Detail", menuName = "Wave Data", order = 1)]
public class WaveData : ScriptableObject
{
    [Space]
    [Header("Wave Details")]
    [SerializeField]
    public int waveIndex = -1;
    
    [Space]
    [Header("Wave Timer Details")]
    [SerializeField]
    [Tooltip("The time in minutes before a wave starts")]
    public float timeBeforeWaveStarts = 0;

    [Space]
    [Header("Wave Data")]
    [SerializeField]
    public List<EnemyAmountPerWave> enemiesInWave = new List<EnemyAmountPerWave>();
}

[System.Serializable]
public class EnemyAmountPerWave
{
    public EnemyType enemyType;
    public int numberOfEnemies = 0;
}
