using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    private ControllerInput controllerInput;

    
    // Start is called before the first frame update
    void Awake()
    {
        controllerInput = new ControllerInput();

    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnEnable() {
        controllerInput.Enable();
    }
    public void OnDisable() {
        controllerInput.Disable();
    }

    public Vector2 GetPlayerMovement(){
        return controllerInput.Player.Movement.ReadValue<Vector2>();
    }

    public Vector2 GetMouseMovement(){
        return controllerInput.Player.Look.ReadValue<Vector2>();
    }

    public float GetCrouch()
    {
        return controllerInput.Player.Crouch.ReadValue<float>();
    }

    public float GetSprint()
    {
        return controllerInput.Player.Sprint.ReadValue<float>();
    }
        
    public bool GetFlash()
    {
        return controllerInput.Player.Flashlight.ReadValue<float>() == 1 && controllerInput.Player.Flashlight.triggered;
    }

    public bool GetInteract()
    {
        return controllerInput.Player.Interact.ReadValue<float>() == 1 && controllerInput.Player.Interact.triggered;
    }

}
