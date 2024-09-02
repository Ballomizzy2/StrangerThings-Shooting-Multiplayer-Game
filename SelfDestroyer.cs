using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SelfDestroyer : MonoBehaviour
{
    PhotonView view;
    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void DestroySelf(float delay = 0)
    {
       PhotonNetwork.Destroy(gameObject);
    }
}
