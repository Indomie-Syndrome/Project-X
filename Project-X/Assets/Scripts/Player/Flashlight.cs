using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    private bool shouldFlash => inputManager.GetFlash() && canFlash;

    [Header("External Object")]
    [SerializeField] private GameObject flashLight;
    [SerializeField] private AudioSource flashlightSound;
    [SerializeField] private InputManager inputManager;



    [SerializeField] private float maxEnergy;
    [SerializeField] private float energyDecrement;
    private bool canFlash => curEnergy > 0;
    private float curEnergy;
    private bool flashlightActive;

    // Start is called before the first frame update
    void Start()
    {
        curEnergy = maxEnergy;
        flashlightActive = false;
        flashLight.SetActive(flashlightActive);
    }

    // Update is called once per frame
    void Update()
    {
        if(shouldFlash)
        {
            flashlightSound.Play();
            flashlightActive = !flashlightActive;
            flashLight.SetActive(flashlightActive);
        } 

        HandleFlashlightUse();


    }

    

    public void AddEnergy(float energy)
    {
        curEnergy += energy;
    }

    private void HandleFlashlightUse()
    {
        if(flashlightActive && canFlash)
        {

            curEnergy -= energyDecrement * Time.deltaTime;
            if(curEnergy <= 0)
            {
                curEnergy= 0;
                flashlightActive = false;
                flashLight.SetActive(flashlightActive);
            }
        }
    }
}
