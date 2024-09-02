using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    public void SpawnChest()
    {
        SpawnManager.SpawnANetworkObject("chest", transform.position, transform.rotation);
        SpawnManager.DestroyNetworkObject(gameObject);
    }
}
