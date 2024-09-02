using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private List<Transform> spawnPoints = new List<Transform>();
    

    [SerializeField]
    private GameObject playerPrefab;

    private GameManager _gameManager;

    private PhotonView _view;

    private static GameObject gameObjectToDestroy;
    private static float destroyTime;

    public List<StarterAssets.ThirdPersonController> allPlayers = new List<StarterAssets.ThirdPersonController>();

    private void Start()
    {
        _view = GetComponent<PhotonView>();
        _gameManager = FindObjectOfType<GameManager>();
        SpawnPlayer();

    }

    // spawn the players in different spawn points
    private void SpawnPlayer()
    {
        if (true)
        {
            //spawn player at random spawn point
            Transform spawnPointGO = spawnPoints[Random.Range(0, spawnPoints.Count)];
            Vector3 spawnPoint = spawnPointGO.transform.position;

            float randomOffset = Random.Range(0.0f, 1.0f);
            Vector3 playerSpawnPos = new Vector3(spawnPoint.x + randomOffset, spawnPoint.y, spawnPoint.z + randomOffset);

            if (playerPrefab != null)
            {
                GameObject newPlayer = PhotonNetwork.Instantiate(playerPrefab.name, playerSpawnPos, spawnPointGO.rotation);
                newPlayer.GetComponent<PhotonView>().Owner.TagObject = newPlayer;

                if(_gameManager)_gameManager.AddPlayer(newPlayer.GetComponentInChildren<StarterAssets.ThirdPersonController>());
            }
        }
    }

    public GameObject SpawnNetworkObject(string objectName, Vector3 pos, Quaternion quat)
    {
        return PhotonNetwork.Instantiate(objectName, pos, quat);
    }

    public static GameObject SpawnANetworkObject(string objectName, Vector3 pos, Quaternion quat)
    {
        return PhotonNetwork.Instantiate(objectName, pos, quat);
    }

    public static void DestroyNetworkObject(GameObject go)
    {
        PhotonNetwork.Destroy(go);
    }

    public void DestroyNetworkObjectInSeconds(GameObject go, float delayTime)
    {
        gameObjectToDestroy = go;
        destroyTime = delayTime;

        IEnumerator coroutine = IDestroyNetworkObject(go);
        StartCoroutine(coroutine);
    }

    private IEnumerator IDestroyNetworkObject(GameObject go)
    {
        yield return new WaitForSeconds(destroyTime);
        DestroyNetworkObject(go);
        //gameObjectToDestroy = null;
        //destroyTime = 0;
    }
}
