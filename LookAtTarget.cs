using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    [SerializeField]
    Transform target;

    private void Update()
    {

        if(target == null)
        {
            target = GameObject.Find("PlayerFollowCamera").transform;
        }
        else
        {
            float xRot, zRot;
            xRot = transform.localEulerAngles.x;
            zRot = transform.localEulerAngles.z;
            transform.LookAt(target);
            transform.localEulerAngles = new Vector3(xRot, transform.localEulerAngles.y, zRot);

            //transform.forward = -target.forward;
        }
    }
}
