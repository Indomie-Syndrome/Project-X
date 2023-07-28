using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerMovement : MonoBehaviour
{

    private CharacterController characterController;
    public bool CanMove { get; private set; } = true;

    private bool shouldCrouch => inputManager.GetCrouch() == 1 && !inCrouchAnimation && isGrounded && canCrouch; 
    private bool isSprinting => canSprint && inputManager.GetSprint() == 1 && playerInput != Vector2.zero;

    [Header("External Scripts")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private CinemachineVirtualCamera playerCamera;
    [SerializeField] private Transform playerModel;

    [Header("Movement Boolean")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool useStamina = true;
    [SerializeField] private bool useFootsteps = true;

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

    // Player Movement reference
    private Vector3 playerMoveDir;

    // For smoothing motion
    private Vector2 curVelocityRef = Vector2.zero;
    private Vector2 currentDir = Vector2.zero;

    private Vector2 playerInput => inputManager.GetPlayerMovement();

    [Header("Sprint Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaUse = 5;
    [SerializeField] private float timeBeforeRegenerate = 5;
    [SerializeField] private float staminaIncrement = 3;
    [SerializeField] private float timeIncrement = 2;
    private float currentStamina;
    private Coroutine regenerateStamina;

    [Header("Mouse")]
    public float mouseSensitivity = 50f;
    private float rotationX = 0f;


    [Header("Crouch")]
    [SerializeField] private float crouchSpeed = 3;
    [SerializeField] private float crouchDuration = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(1,0.6f,1);
    [SerializeField] private Vector3 standingCenter = new Vector3(1, 1, 1);
    private bool isCrouching;
    private bool inCrouchAnimation;


    [Header("Footsteps")]
    [SerializeField] private float baseStepsSpeed = 0.5f;
    [SerializeField] private float crouchStepMultplier = 1.5f;
    [SerializeField] private float sprintStepMultiplier = 0.6f;
    [SerializeField] private AudioSource footStepsAudioSource = default;
    [SerializeField] private AudioClip[] woodClips =default;
    private float footStepTimer = 0;
    private float GetCurrentOffest => isCrouching ? baseStepsSpeed * crouchStepMultplier : isSprinting ? baseStepsSpeed * sprintStepMultiplier : baseStepsSpeed;


    




    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible= false;
        playerCamera.m_Lens.FieldOfView = defaultFov;
        currentStamina = maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = characterController.isGrounded;
        if(CanMove)
        {
            HandleMovementInput();

            if(canCrouch) HandleCrouch();

            if (useStamina) HandleStamina();

            ApplyFinalMovement();
        }
        Debug.Log(currentStamina);



    }
    void LateUpdate()
    {
        CameraLook();
    }



    private void HandleMovementInput()
    {
        currentDir = Vector2.SmoothDamp(currentDir, playerInput, ref curVelocityRef, moveSmoothTime); // damping movement
        float targetSpeed = isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : walkingSpeed;
        
        float movementDirY = playerMoveDir.y;
        playerMoveDir = (transform.TransformDirection(Vector3.forward) * currentDir.y * targetSpeed) + (transform.TransformDirection(Vector3.right) * currentDir.x * targetSpeed) ; 
        playerMoveDir.y = movementDirY;

    }

    private void ApplyFinalMovement()
    {
        playerMoveDir.y -= gravity * Time.deltaTime;
        if (isGrounded && playerMoveDir.y < 0)
        {
            playerMoveDir.y = -2f;
        }

        // handle camera zoom
        if (isSprinting) ZoomCamera(sprintFOV);
        else ZoomCamera(defaultFov);

        characterController.Move(playerMoveDir * Time.deltaTime);
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
        playerCamera.m_Lens.FieldOfView = Mathf.MoveTowards(playerCamera.m_Lens.FieldOfView, target, angle / zoomDuration * Time.deltaTime);
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

    private void HandleStamina()
    {
        if(isSprinting && !isCrouching)
        {
            if (regenerateStamina != null)
            {
                StopCoroutine(regenerateStamina);
                regenerateStamina = null;
            }
            currentStamina -= staminaUse * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                canSprint = false;
            }
        }

        if(!isSprinting && currentStamina < maxStamina && regenerateStamina == null)
        {
            regenerateStamina = StartCoroutine(RegenerateStamina());
        }
        
    }

    private IEnumerator RegenerateStamina()
    {
        yield return new WaitForSeconds(timeBeforeRegenerate);
        WaitForSeconds timeToWait = new WaitForSeconds(timeIncrement);

        while (currentStamina < maxStamina)
        {
            if(currentStamina > 0)
            {
                canSprint = true;
            }

            
            if(currentStamina > maxStamina)
            {
                currentStamina = maxStamina;
            }

            currentStamina += staminaIncrement;

            yield return timeToWait;
        }

        regenerateStamina = null;
    }

    private void HandleFootsteps()
    {
        if (!isGrounded) return;
        if (playerInput == Vector2.zero) return;

        footStepTimer = Time.deltaTime;
        if(footStepTimer == 0) 
        {
            if (Physics.Raycast(playerCamera.transform.position, Vector3.down,out RaycastHit hit, 3f))
            {
                switch(hit.collider.tag)
                {
                    case "Footsteps/Wood":
                        break;
                    default: 
                        break;
                }
            }
        }
    }



}
