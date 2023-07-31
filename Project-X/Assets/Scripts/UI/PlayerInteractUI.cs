using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInteractUI : MonoBehaviour
{
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private TextMeshProUGUI interactText;

    private void Update()
    {
        if (playerInteract.GetInteractable() != null)
        {
            Show(playerInteract.GetInteractable());
            Debug.Log(playerInteract.GetInteractable().GetInteractText());
        }
        else
        {
            Hide();
        }
    }
    private void Show(IInteractable interactable)
    {
        
        interactText.text = "Pickup " + interactable.GetInteractText();
    }
    private void Hide()
    {
        interactText.text = null;
    }

}
