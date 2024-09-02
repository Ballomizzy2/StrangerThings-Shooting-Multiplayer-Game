using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
public class Inventory : MonoBehaviourPunCallbacks
{
    [SerializeField]
    public GameObject[] items = new GameObject[4];

    public int selectedItemIndex = 0;
    [SerializeField]
    private GameObject currentItem;

    public WeaponManager weaponManager;

    [SerializeField]
    private StarterAssets.StarterAssetsInputs input;
    [SerializeField]
    private PhotonView view;

    private NetworkManager networkManager;

    private void Awake()
    {
        networkManager = FindObjectOfType<NetworkManager>();
    }

    public void ChangeCharacter(StarterAssets.StarterAssetsInputs _input, WeaponManager _weaponManager)
    {
        input = _input;
        this.weaponManager = _weaponManager;
    }


    private void Update()
    {
        int i;
        if (MyPlayer())
        {
            // switch items
            if (Input.GetKeyDown(KeyCode.Alpha1)) 
            {
                i = 1;
                SwitchItem(i);
                if (MyPlayer())
                {
                    Hashtable hash = new Hashtable();
                    hash.Add("WeaponToSwitchTo", i);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                i = 2;
                SwitchItem(i);
                if (MyPlayer())
                {
                    Hashtable hash = new Hashtable();
                    hash.Add("WeaponToSwitchTo", i);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                i = 3;
                SwitchItem(i);
                if (MyPlayer())
                {
                    Hashtable hash = new Hashtable();
                    hash.Add("WeaponToSwitchTo", i);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                i = 4;
                SwitchItem(i);
                if (MyPlayer())
                {
                    Hashtable hash = new Hashtable();
                    hash.Add("WeaponToSwitchTo", i);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
                }
            }
        }
    }

    private void SwitchItem(int _toIndex)
    {
        Debug.Log("TRIED TO SWITCH ITEM! TO" + _toIndex);
        //view.RPC("RPC_SwitchItem", RpcTarget.AllBuffered, _toIndex);
        RPC_SwitchItem(_toIndex);

    }
    [PunRPC]
    private void RPC_SwitchItem(int _toIndex)
    {
        Debug.Log("Tried switching!" + " with index " + _toIndex);
        // _toIndex takes the int straight from the input (1-4),
        // so minus one to set to array system
        _toIndex--;
        // if item already selected, do nothing
        /*if (selectedItemIndex == _toIndex)
            return;*/
        // disable all other items and enable the indexed one
        for (int i = 0; i < items.Length; i++)
        {
            if (items[_toIndex] != null)
            {
                Debug.Log(i + " is null");
                if (i == _toIndex)
                {
                    Debug.Log(i + "still gets called");
                    currentItem = items[i].gameObject;
                    currentItem.SetActive(true);
                    Debug.Log("Also tried Activating");
                    // also update this on the server
                    //networkManager.SetNetworkObjectActive(currentItem, true);
                    if (items[_toIndex] && currentItem.CompareTag("Weapon"))
                    {
                        weaponManager.SwitchWeapon(currentItem.GetComponent<WeaponController>());
                        // update selected index
                        selectedItemIndex = _toIndex;
                    }
                }
                else
                {
                    if (items[i])
                    {
                        items[i].gameObject.SetActive(false);
                        // also update this on the server
                        //networkManager.SetNetworkObjectActive(items[i].gameObject, false);

                    }
                }

            }
        }
    }

    public void PickUpItem(GameObject _item, bool _isWeapon)
    {
        int _itemPV_ID = _item.GetComponent<PhotonView>().ViewID;
        //view.RPC("RPC_PickUpItem", RpcTarget.AllBuffered, _itemPV_ID, _isWeapon);
        Debug.Log("INVENTORY GOT MESSAGE AND TRIEND PICKING UP AGAIN!");
        RPC_PickUpItem(_itemPV_ID, _isWeapon);
    }

    [PunRPC]
    public void RPC_PickUpItem(int _itemPV_ID, bool _isWeapon)
    {
        Debug.Log("IBNVENTORY PICKED UP ITEM!");
        GameObject _item = PhotonView.Find(_itemPV_ID).gameObject;
        Debug.Log(_item.name + " IS THE ITEM WE WANT TO STORE");
        // drop item on floor

        // check for next free spot 
        int index = 0;
        bool freeSpot = false;
        for(index = 0; index < items.Length; index++)
        {
            if (items[index] == null)
            {
                freeSpot = true;
                break;
            }
        }
        if (!freeSpot)
            index = selectedItemIndex;
        GameObject obj = items[index];

        // change item with index
        if (_item.CompareTag("Weapon"))
        {

            // if something, drop what was there before picking 
            if (!freeSpot)
            {
                //obj = Instantiate(obj);
                obj = SpawnManager.SpawnANetworkObject(obj.name, transform.position, Quaternion.identity);
                obj.GetComponent<WeaponController>().Drop(_item.transform.position, _item.transform.localScale);
            }
            obj = _item;
            // draw the right weapon with data 
            WeaponController weaponController = obj.GetComponent<WeaponController>();
            if (weaponController != null)
            {
                items[index] = weaponManager.SwitchWeapon(weaponController);
                Debug.Log(items[index].gameObject.name + " IS THE PICKED UP GAMEOBJECT STORED ALREADY!");
                Destroy(_item);
                //view.RPC("DestroyObjectOnMasterClient", RpcTarget.MasterClient, _item.GetComponent<PhotonView>().ViewID);
                //PhotonNetwork.Destroy(_item);
                //_item.GetComponent<PhotonView>().RPC("DestroySelf", RpcTarget.AllBuffered);
            }
        }
        else if (_item.CompareTag("Ammo"))
        {

        }
        else if (_item.CompareTag("Boost"))
        {

        }
        // if there the item picked got stored NOT in the current slot, switch to the current item (literally do nothing)
        if(items[selectedItemIndex] != null)
            SwitchItem(selectedItemIndex + 1);
        else
            SwitchItem(index + 1);
    }

    [PunRPC]
    private void DestroyObjectOnMasterClient(int viewID)
    {
        GameObject obj = PhotonView.Find(viewID).gameObject;
        PhotonNetwork.Destroy(obj);
        Debug.Log("Tried to destroy Object");
    }

    private void DropItem(int _index, bool _dropAllItems = false)
    {
        if (_dropAllItems)
        {
            // drop all items
            for(int i = 0 ; i < items.Length; i++)
            {
                items[i].transform.SetParent(null);
            }
            items = null;
        }
        else
        {
            // drop from transform
            items[_index].transform.SetParent(null);
            // drop from list
            items[_index] = null;
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //Debug.Log("Player Properties for switxhing checked!");
        if (!view.IsMine && targetPlayer == photonView.Owner)
        {
            // switch up weapons update around server
            if (changedProps.ContainsKey("WeaponToSwitchTo"))
            {
                SwitchItem((int)changedProps["WeaponToSwitchTo"]);
                Debug.Log("Tried switching!");
            }

        }
    }

    private bool MyPlayer()
    {
        return view.IsMine;
    }
}

public class Item
{
    public GameObject _itemGameObject;
    public ItemTyoe _itemTyoe;
}

public enum ItemTyoe
{
    Weapon, Ammo, Boost
}
