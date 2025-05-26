using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

namespace Universe
{
    public class Grid : MonoBehaviour
    {
        //Gamespeed Variables
        public float gameSpeed = 1f;
        public float storedGameSpeed = 1f;
        public GameObject gameSpeedUI;
        //public GameObject CurrentTimeUI;
        private double timeInSeconds;
        private string currentTime;

        public void Start()
        {
            Time.timeScale = 1f;
            //call the repeating timer to update the gametime
            InvokeRepeating("GameTimer", 1f, 1f);
            SetGameSpeed();
        }

        public void SetGameSpeed()
        {
            // Takes slider value and sets as gamespeed
            gameSpeed = GameObject.Find("Time_Slider").GetComponent<Slider>().value;
            storedGameSpeed = gameSpeed;
            Time.timeScale = gameSpeed; 
            gameSpeedUI.GetComponent<Text>().text = gameSpeed.ToString() + "x";
        }

        public void PauseUnpause()
        {
            // Method to toggle timescale to 0 when called
            if (gameSpeed == 0)
            {
                gameSpeed = storedGameSpeed;
                gameSpeedUI.GetComponent<Text>().text = gameSpeed.ToString() + "x";
            } else {
                gameSpeed = 0;
                gameSpeedUI.GetComponent<Text>().text = "||";
            }
            Time.timeScale = gameSpeed;
            Logger.Log("Timekeeping", $"Setting Timescale to {gameSpeed}");
        }
        public void FixedUpdate()
        {
            GameTimer();
            //DisplayDate();
        }

        void DisplayDate()
        {
            //CurrentTimeUI.GetComponent<TextMesh>().text = currentTime.ToString();
        }

        private void GameTimer()
        {
            // Iterate the game time upwards
            timeInSeconds += gameSpeed * Time.deltaTime;

            // Get display values from the saveclock
            int Days = TimeSpan.FromSeconds(timeInSeconds).Days;
            int Hours = TimeSpan.FromSeconds(timeInSeconds).Hours;
            int Minutes = TimeSpan.FromSeconds(timeInSeconds).Minutes;
            int Seconds = TimeSpan.FromSeconds(timeInSeconds).Seconds;
            // Calculate years as TimeSpan doesn't have that feature
            int Years = Math.DivRem(Days, 365, out Days);

            currentTime =
                "T+" + Years.ToString()
                + "y:" + Days.ToString("000")
                + "d:" + Hours.ToString("00")
                + "h:" + Minutes.ToString("00")
                + "m:" + Seconds.ToString("00") + "s";
        }

        public double TimeInSeconds
        {
            get { return timeInSeconds; }
        }
    }
}
