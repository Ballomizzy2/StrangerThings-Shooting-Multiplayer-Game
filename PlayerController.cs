using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using StarterAssets;
using Cinemachine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    private PhotonView _view;

    [SerializeField]
    private PlayerInput _input;
    [SerializeField]
    private StarterAssetsInputs _starterAssetsInput;
    [SerializeField]
    private CinemachineVirtualCamera _vCam;
    [SerializeField]
    private CinemachineVirtualCamera _vAimCam;

    [SerializeField]
    private CharacterType selectedCharacter;
    [SerializeField]
    private GameObject currentCharacter;
    private CharacterManager characterManager;

    [SerializeField]
    private List<GameObject> characterList = new List<GameObject>();



    private void Awake()
    {
        _view = GetComponent<PhotonView>();
        characterManager = currentCharacter.GetComponent<CharacterManager>();
        SwitchCharacter(selectedCharacter);

        _input.enabled = true;
        _starterAssetsInput.enabled = true;

        if (!MyPlayer())
        {
            Destroy(GetComponentInChildren<CinemachineBrain>());
            Destroy(GetComponentInChildren<CinemachineVirtualCamera>());
            Destroy(GetComponentInChildren<Camera>());
            
            GetComponentInChildren<PlayerInput>().enabled = false;
            Destroy(GetComponentInChildren<StarterAssetsInputs>());

        }

    }

    public StarterAssets.StarterAssetsInputs GetInput()
    {
        return _starterAssetsInput;
    }

    public PlayerCameraShake GetCameraPlayerShake()
    {
        if(_vCam)
            return _vCam.GetComponent<PlayerCameraShake>();
        else
            return null;
    }
    public PlayerCameraShake GetAimCameraPlayerShake()
    {
        if (_vAimCam)
            return _vAimCam.GetComponent<PlayerCameraShake>();
        else
            return null;
    }


    private bool MyPlayer()
    {
        if (_view.IsMine)
            return true;
        else
            return false;
    }

    private void SwitchCharacter(CharacterType character) 
    {
        switch (character)
        {
            case CharacterType.PlayerBody:
                SpawnCharacter("PlayerBody");
                break;
            case CharacterType.El:
                SpawnCharacter("El");
                break;
            default:
                break;
        }
    }

    private enum CharacterType
    {
        PlayerBody, El
    };

    private void SpawnCharacter(string characterName)
    {
        GameObject newCharacter = null;// = SpawnManager.SpawnANetworkObject(characterName, currentCharacter.transform.position, currentCharacter.transform.rotation);
        foreach(GameObject go in characterList)
        {
            if(go.name == characterName)
            {
                newCharacter = go;
                go.SetActive(true);
            }
        }
        if (newCharacter == null)
            return;
        newCharacter.transform.parent = transform;

        characterManager.AddMyComponentsToAnotherCharacter(newCharacter.GetComponent<CharacterManager>());


        //Settle Inputs
        _input = newCharacter.GetComponent<PlayerInput>();
        _starterAssetsInput = newCharacter.GetComponent<StarterAssetsInputs>();


        //currentCharacter.SetActive(false);
        SpawnManager.DestroyNetworkObject(currentCharacter);

        currentCharacter = newCharacter;
    }
}
