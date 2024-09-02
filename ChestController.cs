using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ChestController : MonoBehaviour
{
    // all rewardable items
    [SerializeField]
    private List<GameObject> items = new List<GameObject>();
    GameObject go;
    [SerializeField]
    Transform point;
    int randomNumber;
    Animator animator;
    PhotonView view;
    bool spawnItem;
    SpawnManager spawnManager;

    [SerializeField]
    private GameObject chestSpawner;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        view = GetComponent<PhotonView>();
        spawnManager = FindObjectOfType<SpawnManager>();
    }
    public void OpenChest(int index, bool _spawnItem)
    {
        Debug.Log("Opemed");
        randomNumber = index;
        spawnItem = _spawnItem;
        animator.SetBool("OpenChest", true);
    }

   
    private void SpawnItem(GameObject itemToSpawn)
    {
        if (spawnItem)
        {
            go = spawnManager.SpawnNetworkObject(itemToSpawn.name, point.position, Quaternion.identity);
            //PhotonNetwork.Instantiate(itemToSpawn.name, point.position, Quaternion.identity);
            //go.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.MasterClient);
            animator.SetBool("Destroy", true);
            //DestroyChestAfterSeconds(3);
        }
    }
    public void DestroyChest()
    {
        //Destroy(gameObject);
        chestSpawner.transform.parent = null;
        SpawnManager.DestroyNetworkObject(gameObject);
    }

    private IEnumerator DestroyChestAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    private void OnDestroy()
    {
        chestSpawner.transform.parent = null;
    }

    public int GetRandomIntFromChest()
    {
        return Random.Range(0, items.Count);
    }
    public void SpawnItemTrigger()
    {
        SpawnItem(items[randomNumber]);
    }

    private bool MyPlayer()
    {
        return view.IsMine;
    }
}
