using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float playerSpeed = 10f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PlayerWalk();
    }

    void PlayerWalk() {
        float dirX = Input.GetAxis("Horizontal");
        float dirZ = Input.GetAxis("Vertical");
        Vector3 dirMove = transform.right * dirX + transform.forward * dirZ;
        dirMove = dirMove.normalized;
        controller.Move(dirMove * playerSpeed * Time.deltaTime);
    }
}
