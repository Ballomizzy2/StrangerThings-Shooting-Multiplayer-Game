using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTerrainManager : MonoBehaviour
{
    Terrain terrain;
    TerrainCollider terrainCollider;

    private void Start()
    {
        terrain = GetComponent<Terrain>();
        terrainCollider = GetComponent<TerrainCollider>();

        SetTerrainColliders();
    }

    private void SetTerrainColliders()
    {
        terrainCollider.enabled = false;
        terrainCollider.enabled = true;
    }
}
