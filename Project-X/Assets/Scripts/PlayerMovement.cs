using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerMovement : MonoBehaviour
{

    private CharacterController characterController;
    private Vector3 playerVelocity;

    private enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    private MovementState movementState;

    [Header("External Scripts")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Camera playerCamera;


    [Header("Walking Variables")]
    public float walkingSpeed;
    public float sprintSpeed;
    public float gravity = 9.8f;
    public float zoomMultiplier = 2;
    public float defaultFov = 60;
    public float zoomDuration = 2;
    public float sprintFOV = 70;
    private bool isGrounded;


    [Header("Mouse")]
    public float mouseSensitivity = 50f;
    

    private float rotationX = 0f;




    void Start()
    {
        characterController = GetComponent<CharacterController>();
        inputManager.OnEnable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible= false;
        playerCamera.fieldOfView = defaultFov;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = characterController.isGrounded;
        MovePlayer();


    }
    void LateUpdate()
    {
        CameraLook();
    }



    private void MovePlayer()
    {
        Vector2 playerInput = inputManager.GetPlayerMovement();
        Vector3 moveDir = new Vector3(playerInput.x, 0f, playerInput.y);
        

        if (inputManager.GetSprint() == 0)
        {
            ZoomCamera(defaultFov);
            characterController.Move(transform.TransformDirection(moveDir) * walkingSpeed * Time.deltaTime);
        }

        // handle sprinting
        else if(inputManager.GetSprint() == 1 && (playerInput.y != 0 || playerInput.x !=0 ) )
        {
            ZoomCamera(sprintFOV);
            characterController.Move(transform.TransformDirection(moveDir) * sprintSpeed * Time.deltaTime);
        }

        playerVelocity.y -= gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        characterController.Move(playerVelocity * Time.deltaTime);

    }


    private void CameraLook()
    {
        Vector2 mouseDir = inputManager.GetMouseMovement();
        rotationX -= (mouseDir.y * Time.deltaTime) * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        // rotate camera
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX,0,0) ;

        // rotate player
        transform.Rotate(Vector3.up * (mouseDir.x * Time.deltaTime) * mouseSensitivity);


    }

    // sprint zoom
    void ZoomCamera(float target)
    {
        float angle = Mathf.Abs((defaultFov / zoomMultiplier) - defaultFov);
        playerCamera.fieldOfView = Mathf.MoveTowards(playerCamera.fieldOfView, target, angle / zoomDuration * Time.deltaTime);
    }
}
