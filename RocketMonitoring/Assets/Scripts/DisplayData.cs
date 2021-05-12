﻿using UnityEngine;
using System.IO.Ports;
using UnityEngine.UI;
using TMPro;


// DISPLAYED DATA
// LAT LONG A V FRT SND LAT_B LONG_B
// FRT SND WILL TRIGGER ACTIONS ON THE ROCKET
// ANGLE IS A STATIC VARIABLE ON THE ROCKETCONTROLLER, directions

// DATA ORDER AND INDEXES, IN STRING ARRAY
/*
    LAT     0
    LONG    1 
    A       2
    V       3
    AN      4
    FRT     5
    SND     6
    LAT_B   7
    LONG_B  8
*/

public class DisplayData : MonoBehaviour
{
    // com and baud rate later will initially selected 
    SerialPort sp = new SerialPort("COM4", 9600);

    bool readAvailable = true;

    // make sure this matches with arduino data transmission freq 
    float readPeriodRemaining;
    [Header("Data Reading Period")]
    [SerializeField]
    float readPeriod = 0.2f;

    [Header("Velocity Parameters")]
    [SerializeField]
    SpeedometerController speedometer;

    [Header("Altitude Parameters")]
    [SerializeField]
    TextMeshProUGUI textAltitude;

    void Start()
    {
        sp.Open();
        sp.ReadTimeout = 1;
        readPeriodRemaining = readPeriod;
    }

    void Update()
    {
        readPeriodRemaining -= Time.deltaTime;
        if (readPeriodRemaining <= 0f)
        {
            readPeriodRemaining = readPeriod;
            readAvailable = true;
        }

        if (readAvailable)
        {
            readAvailable = false;
        }
        else
            return;

        if (sp.IsOpen)
        {
            try
            {
                string receivedData = sp.ReadLine();
                string[] datas = receivedData.Split(':');

                // data size is currently 9, check if it is
                // also check if string length is proper, 55 for now
                bool checkValidFirst = false;
                bool checkValidSecond = false;
                if(receivedData.Length > 55)
                {
                    checkValidFirst = true;
                    if (datas.Length == 9)
                        checkValidSecond = true;
                }

                // if data is valid, do something
                if (checkValidFirst && checkValidSecond)
                {
                    // altitude display, convert float then format to proper string
                    float altitudeData = float.Parse(datas[2]) / 100f;
                    textAltitude.text = altitudeData.ToString();

                    // velocity unit conversion and set on speedometer, 3
                    float speedData_meters = float.Parse(datas[3]) / 100f;
                    float speedData_km = speedData_meters * 3600f / 1000f;
                    speedometer.SetSpeed(speedData_km);

                    // assign rotation directions string on the RocketController.cs, 4
                    string[] RPstrings = datas[4].Split(',');
                    RocketController.instance.RotateRocket(RPstrings[0], RPstrings[1]);
                }
            }
            catch (System.Exception)
            {

            }
        }
    }
}


/* TEXT DISPLAY CODE
 * 
    // display lat and long, 0 and 1
    textRocketLatLong.text = "" + "Rocket Coordinates\n";
    textRocketLatLong.text += datas[0] + "\n";
    textRocketLatLong.text += datas[1];

    // display altitude and velocity, 2 and 3
    textRocketAltitude.text = "" + "Altitude\n";
    textRocketAltitude.text += datas[2];
    textRocketVelocity.text = "" + "Velocity\n";
    textRocketVelocity.text += datas[3];

    // display first and second parachute, 5 and 6
    if (datas[5] == "0")
        textFirstParachute.color = colorDefaultFrtSnd;
    else
        textFirstParachute.color = colorGreenFrtSnd;

    if (datas[6] == "0")
        textSecondParachute.color = colorDefaultFrtSnd;
    else
        textSecondParachute.color = colorGreenFrtSnd;

    // display base lat and long, 7 and 8
    textBaseLatLong.text = "" + "Base Coordinates\n";
    textBaseLatLong.text += datas[7] + "\n";
    textBaseLatLong.text += datas[8];

 */