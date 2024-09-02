using UnityEngine;
using TMPro;
using Photon.Pun;
public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private TMP_InputField _createRoomInputField, 
                           _joinRoomInputField;


    public void CreateRoom()
    {
        string roomName = _createRoomInputField.text;
        if (IsRoomNameValid(roomName))
            PhotonNetwork.CreateRoom(roomName);
        else
            LogText("Room name is not valid!");
    }
    
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(_joinRoomInputField.text);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Game");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log(message);
    }

    // check for invalid names for servers
    private bool IsRoomNameValid(string _name) 
    {
        bool invalidName = false;
        char space = ' ';
        // check if all letters are just space
        for(int i = 0; i < _name.Length; i++)
        {
            // if player decides to input just spaces, it is invalid
            if (!(_name[i] == space))
                break;
            invalidName = true;
        }
        if (invalidName) return false;
        else return true;
    }

    // use this to print errors to user
    private void LogText(string log)
    {
        Debug.Log(log);
    }
}
