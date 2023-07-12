using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Rewired;

public class PauseMenu : MonoBehaviour
{
    public EventSystem theEventSystem = null; //damn! What an original name my man!
    public GameObject firstSelected = null; //this is the game object that be assigned into the first selcected slot

    public bool isPaused = false;
    public GameObject pauseHolder;
    public GameObject controlScreen; //this is the image that display the controls

    //NOTE FOR PAYTON: All button selection is handled by Unity's Eventsystem manager. You do not need to use code navigate menues unless you want to put yourself through that. Good practice when opening any new page of ui is to have tour script reference the scenes Event system and change the first selected time to the Button or UI element that you want to have selected. 

    void Awake()
    {
        isPaused = false;
    }

    private void OnEnable()
    {
        if (theEventSystem == null) //if the script isnt referencing an event system
            theEventSystem = GameObject.FindObjectOfType<EventSystem>().GetComponent<EventSystem>(); //find the event system in the scene and assign it automaticallty. 

        theEventSystem.firstSelectedGameObject = firstSelected; //set the first selected gameobject to be the part of the UI that is selected first

        pauseHolder.SetActive(false); //the pause holder is false
        Time.timeScale = 1f; //the worlds timescale is 1. 
    }

    public void Update()
    {
        if (theEventSystem == null) //same thing as above but I put it in update jsut incase
            theEventSystem = GameObject.FindObjectOfType<EventSystem>().GetComponent<EventSystem>();
        if (theEventSystem.firstSelectedGameObject == null) //assign the first button automaically basically
            theEventSystem.firstSelectedGameObject = firstSelected; //set the first selected gameobject to be the part of the UI that is selected first

        

    }

    public void MakePause()
    {
        isPaused = true;
        pauseHolder.SetActive(true);
        controlScreen.SetActive(false);
        Time.timeScale = 0f;
    }

    public void MakeUnpaused()
    {
        isPaused = false;
        pauseHolder.SetActive(false);
        controlScreen.SetActive(false);
        Time.timeScale = 1f;
    }

}

