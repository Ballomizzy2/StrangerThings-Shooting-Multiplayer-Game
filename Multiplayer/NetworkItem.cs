using UnityEngine;
using Photon.Pun;

public class NetworkItem : MonoBehaviour
{
    string prefabName;
    PhotonView photonView;
    WeaponController weaponController;
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        weaponController = GetComponent<WeaponController>();
        if (weaponController != null && weaponController.isPickable == false)
            return;
        if(photonView != null && photonView.IsMine)
        {
            prefabName = gameObject.name;
            prefabName = RemoveTextFromString(prefabName, "(Clone)");
            RespawnItem();
        }
    }

    private void RespawnItem()
    {
        Debug.Log("RESPAWNEDDDD!!!!!!!!");
        GameObject go;
        go = PhotonNetwork.Instantiate(prefabName, transform.position, Quaternion.identity);
        if(go != null)
        {
            go.transform.localScale = gameObject.transform.localScale;
            string[] s = prefabName.Split('(');
            //prefabName = RemoveTextFromString(prefabName, "(Clone)");
            go.name = s[0];
            if(go.GetComponent<PhotonView>() == null)
                photonView = go.AddComponent<PhotonView>();
            else
                photonView = go.GetComponent<PhotonView>();


            //Destroy(gameObject, 0.1f);
            photonView.RPC("DestroyObject", RpcTarget.AllBuffered, photonView.ViewID);
        }
    }

    [PunRPC]
    private void DestroyObject(int viewID)
    {
        Destroy(PhotonView.Find(viewID).gameObject);
    }

    private string RemoveTextFromString(string originalText, string textToRemove)
    {
        string newString = originalText;
        for(int i = originalText.Length - 1, x = textToRemove.Length - 1; i > 0; i--, x--)
        {
            if (x > 0 && originalText[i] == textToRemove[x])
            {
                newString.Remove(i, 1);
            }
            else if(x == 0)
                return newString;
            else
                return originalText;
        }
        return newString;
    }
}
