using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPhysicsSimulation : MonoBehaviour
{

    private void OnCollisionStay(Collision collider)
    {
        if (collider.gameObject.CompareTag("Enemy"))
        {
            collider.gameObject.GetComponent<EnemyController>().Die();
        }
    }
}
