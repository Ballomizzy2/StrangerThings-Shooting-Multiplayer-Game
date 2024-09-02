using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;

[RequireComponent(typeof(Animator))]
public class DoorController : MonoBehaviour
{
    private Animator animator;
    private DoorState doorState;
    private PhotonView view;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        view = GetComponent<PhotonView>();
    }

    public void InteractWithDoor()
    {
        if (animator == null)
            return;
        switch (doorState)
        {
            case DoorState.Close:
                OpenDoor();
                doorState = DoorState.Open;
                break;
            case DoorState.Open:
                CloseDoor();
                doorState = DoorState.Close;
                break;
            default:
                break;
        }
    }
    private void OpenDoor()
    {
        animator.SetTrigger("OpenDoor");
        StartCoroutine(IE_ToggleDoorColliders(false));
    }

    private void CloseDoor()
    {
        animator.SetTrigger("CloseDoor");
        StartCoroutine(IE_ToggleDoorColliders(false));
    }

    private IEnumerator IE_ToggleDoorColliders(bool toValue)
    {
        ToggleDoorColliders(toValue);
        yield return new WaitForSeconds(1);
        ToggleDoorColliders(!toValue);
    }
    private void ToggleDoorColliders(bool b)
    {
        Collider[] doorColliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in doorColliders)
        {
            collider.enabled = b;
        }
    }
    private enum DoorState
    {
        Close, Open
    }

    private bool MyPlayer()
    {
        return view.IsMine;
    }
}

