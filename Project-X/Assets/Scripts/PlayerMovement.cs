using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerMovement : MonoBehaviour
{

    private CharacterController characterController;
    
    private bool shouldCrouch => inputManager.GetCrouch() == 1 && !inCrouchAnimation && characterController.isGrounded; 

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
    [SerializeField] private Transform playerModel;


    [Header("Walking Variables")]
    public float walkingSpeed;
    public float sprintSpeed;
    public float gravity = 9.8f;
    public float zoomMultiplier = 2;
    public float defaultFov = 60;
    public float zoomDuration = 2;
    public float sprintFOV = 70;
    [SerializeField][Range(0f, 0.5f)] private float moveSmoothTime = 0.3f;
    private bool isGrounded;
    // ===============================
    private Vector3 playerVelocity;
    private Vector2 curVelocityRef = Vector2.zero;
    private Vector2 currentDir = Vector2.zero;


    [Header("Mouse")]
    public float mouseSensitivity = 50f;


    [Header("Crouch")]
    [SerializeField] private float crouchSpeed = 3;
    [SerializeField] private float crouchDuration = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(1,0.6f,1);
    [SerializeField] private Vector3 standingCenter = new Vector3(1, 1, 1);
    private bool isCrouching;
    private bool inCrouchAnimation;


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
        HandleCrouch();


    }
    void LateUpdate()
    {
        CameraLook();
    }



    private void MovePlayer()
    {
        Vector2 playerInput = inputManager.GetPlayerMovement();
        currentDir = Vector2.SmoothDamp(currentDir, playerInput, ref curVelocityRef, moveSmoothTime);
        float targetSpeed;
        Vector3 moveDir = new Vector3(currentDir.x, 0f, currentDir.y);

        if (inputManager.GetSprint() == 0)
        {
            ZoomCamera(defaultFov);
            targetSpeed = isCrouching ? crouchSpeed : walkingSpeed;
            characterController.Move(transform.TransformDirection(moveDir) * targetSpeed * Time.deltaTime);
        }

        // handle sprinting
        else if(inputManager.GetSprint() == 1 && (playerInput.y != 0 || playerInput.x !=0 ) )
        {
            if (!isCrouching)
            {
                ZoomCamera(sprintFOV);
            }
            targetSpeed = isCrouching ? crouchSpeed : sprintSpeed;
            characterController.Move(transform.TransformDirection(moveDir) * targetSpeed * Time.deltaTime);
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

    void HandleCrouch()
    {
        if(shouldCrouch)
        {
            StartCoroutine(CrouchStand());
        }
    }

    private IEnumerator CrouchStand()
    {
        // hindari clipping jika ada sesuatu di atas
        if(isCrouching && Physics.BoxCast(playerCamera.transform.position, new Vector3(0.5f,0.5f,0.5f), Vector3.up, Quaternion.Euler(0,0,0),0.5f))
        {

            yield break;
        }

        inCrouchAnimation = true;
        float timelapsed = 0;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = transform.localScale;

        while(timelapsed < crouchDuration)
        {
            transform.localScale = Vector3.Lerp(currentCenter, targetCenter, timelapsed / crouchDuration);
            timelapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetCenter;

        isCrouching = !isCrouching;

        inCrouchAnimation = false;
    }
}
