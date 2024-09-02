using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkManager : MonoBehaviour
{
    private PhotonView view;

    private void Awake()
    {
        view = GetComponent<PhotonView>();
    }

    private void SetAllNetworkObjectsPhotonViewTags()
    {
        PhotonView[] photonView = FindObjectsOfType<PhotonView>();

        foreach (PhotonView p in photonView)
        {
            if (p == null)
                return;
            p.Owner.TagObject = p.gameObject;
        }
    }

    public void SetNetworkObjectActive(GameObject obj, bool state)
    {
        int i = obj.GetComponent<PhotonView>().ViewID;
        view.RPC("RPC_UpdateObjectActiveState", RpcTarget.AllBuffered, i, state);
    }

    [PunRPC]
    private void RPC_UpdateObjectActiveState(int viewID, bool state)
    {
        PhotonView.Find(viewID).gameObject.SetActive(state);
    }

}
