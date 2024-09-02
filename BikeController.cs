using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeController : MonoBehaviour
{
    [SerializeField]
    private MeshRenderer bikeColor;

    [SerializeField]
    private List<Color> bikeColors = new List<Color>();

    private Animator bikeAnimator;
    private Transform _playerBody;
    //private Transform seatPosition;

    private void Awake()
    {
        bikeAnimator = GetComponent<Animator>();

        // set bike to random color
        bikeColor.material.color = bikeColors[Random.Range(0, bikeColors.Count)];
    }

    public void Update()
    {
        //_playerBody.right = transform.right;
        //transform.forward = _playerBody.transform.forward;
        
    }
    public void Ride(Transform player, Transform playerBody)
    {
        bikeAnimator.SetBool("Ride", true);
        playerBody.transform.position = new Vector3(transform.position.x, playerBody.transform.position.y, transform.position.z);
        transform.position += new Vector3(-.3f, 0, 0);
        transform.SetParent(playerBody.transform);
        _playerBody = playerBody;
        //playerBody.SetParent(transform);

    }

    public void Move(bool move, float speed)
    {
        bikeAnimator.SetBool("Ride", move);
        bikeAnimator.SetFloat("Speed", speed);
    }
}
