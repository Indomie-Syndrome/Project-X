using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{

    [Header("External Objects")]
    [SerializeField] private CinemachineVirtualCamera playerCam;
    [SerializeField] private InputManager inputManager;

    [Header("Interact Variables")]
    [SerializeField] private float interactDist = 2f;
    private IInteractable currentInteractable;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        InteractCheck();
        InteractAction();
    }

    private void InteractCheck()
    {
        if(Physics.Raycast(playerCam.transform.position, playerCam.transform.forward, out RaycastHit hitInfo ,interactDist)) 
        {
            if(hitInfo.collider.GetComponent<IInteractable>() != null)
            {
                currentInteractable = hitInfo.collider.GetComponent<IInteractable>();
            }
            else
            {
                currentInteractable = null;
            }
        } else
        {
            currentInteractable= null;  
        }


    }

    private void InteractAction()
    {
        if(currentInteractable != null)
        {
            if(inputManager.GetInteract())
            {
                currentInteractable.Interact(transform);
                Debug.Log("Pickup " + currentInteractable.GetInteractText());    
            }
        }
    }


    public IInteractable GetInteractable()
    {
        return currentInteractable;
    }
}
