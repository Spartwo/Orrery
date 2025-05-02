using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  

public class Timekeep : MonoBehaviour
{

    //Gamespeed Variables
    public float GameSpeed = 1f;
    public GameObject GameSpeedUI;
    public GameObject CurrentTimeUI;
    public float TimeInSeconds;
    private string CurrentTime;

    public void Start()
    {
        Time.timeScale = 1f;
        //call the repeating timer to update the gametime
        //InvokeRepeating("GameTimer", 1f, 1f);
    }
    public void PauseUnpause()
    {   
        //method to toggle timescale to 0 when called
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
        } else {
            Time.timeScale = 0;
            GameSpeedUI.GetComponent<TMP_Text>().text = "Paused";
        }
    }
    public void FixedUpdate()
    {
        //Takes slider value and sets as gamespeed
        GameSpeed = GameObject.Find("Time_Slider").GetComponent<Slider>().value;
        GameTimer();
        DisplayDate();
    }

    void DisplayDate()
    {
        GameSpeedUI.GetComponent<TMP_Text>().text = GameSpeed.ToString() + "x";
        CurrentTimeUI.GetComponent<TMP_Text>().text = CurrentTime.ToString();
    }

    void GameTimer()
    {
        //iterate the game time upwards
        TimeInSeconds += GameSpeed*Time.deltaTime;

        //get display values from the saveclock
        int Days = TimeSpan.FromSeconds(TimeInSeconds).Days;
        int Hours = TimeSpan.FromSeconds(TimeInSeconds).Hours;
        int Minutes = TimeSpan.FromSeconds(TimeInSeconds).Minutes;
        int Seconds = TimeSpan.FromSeconds(TimeInSeconds).Seconds;
        //calculate years as TimeSpan doesn't have that feature
        int Years = Math.DivRem( Days, 365, out Days );

        CurrentTime = 
            "T+" + Years.ToString()
            + "y:" + Days.ToString("000") 
            + "d:" + Hours.ToString("00") 
            + "h:" + Minutes.ToString("00")
            + "m:" + Seconds.ToString("00") + "s";
    }
}
