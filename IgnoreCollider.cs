using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollider : MonoBehaviour
{
    

    [SerializeField]
    private List<LayerMask> layersToIgnore;
    [SerializeField]
    private LayerMask layerInContext;

    /*void Awake()
    {
        foreach(var layer in layersToIgnore)
        {
            Physics.IgnoreLayerCollision(layerInContext.value, layer.value);
        }
    }*/
}
