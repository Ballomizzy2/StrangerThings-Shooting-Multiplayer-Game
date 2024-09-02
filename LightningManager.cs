using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] lightningPrefabs;

    [SerializeField]
    public Color[] lightningColors;
    private GameObject[] lightningObjectPool;

    private Collider bounds;

    private Transform player;

    private void Start()
    {
        bounds = GetComponent<Collider>();
        
    }

    private void Update()
    {
        if(!player)
            player = FindObjectOfType<StarterAssets.ThirdPersonController>().transform;
        else
            FlashThunder();
    }

    private void FlashThunder()
    {
        // get random position within bounds
        float xMin, xMax, xVal,
              yMin, yMax, yVal,
              zMin, zMax, zVal,
              xRot, yRot, zRot, wRot;

        xMin = bounds.bounds.min.x;
        xMax = bounds.bounds.max.x;
        yMin = bounds.bounds.min.y;
        yMax = bounds.bounds.max.y;
        zMin = bounds.bounds.min.z;
        zMax = bounds.bounds.max.z;

        xVal = Random.Range(xMin, xMax);
        yVal = Random.Range(yMin, yMax);
        zVal = Random.Range(zMin, zMax);

        xRot = Random.Range(0, 360);
        yRot = Random.Range(0, 360);
        zRot = Random.Range(0, 360);
        wRot = Random.Range(0f, 1f);

        Vector3 newPos = new Vector3(xVal, yVal, zVal);   

        // spawn random type of thunder
        GameObject newLighning = Instantiate(lightningPrefabs[Random.Range(0, lightningPrefabs.Length)], newPos, new Quaternion(xRot, yRot, zRot, 1));
        newLighning.transform.LookAt(player);
        Destroy(newLighning, 1f);

    }
}
