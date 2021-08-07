﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO.Ports;
using System;
using System.Linq;

public class EntryManager : MonoBehaviour
{
    // static datas to input main scene elements
    public static string dataCOM = "";
    public static int dataBaudRate = 9600;
    public static float dataObtainPeriod = 500f;
    public static string warningString;
    public static string flightRecordsPath = "";
    public static bool isMouseOverPathText = false;

    [SerializeField]
    TMP_Dropdown dropDown_Ports;

    [SerializeField] 
    TMP_Dropdown dropDown_BaudRates;

    [SerializeField]
    TMP_InputField inputField_Period;

    [SerializeField]
    GameObject warningToolTip;

    [SerializeField]
    TextMeshProUGUI pathText;

    [SerializeField]
    Canvas canvas;

    // check bools
    bool checkCOMPort;
    bool checkDataPeriod;

    // entry parameters
    List<string> ports;

    // tooltip for flight record path text area
    GameObject pathTooltip;

    void Start()
    {
        // get available ports
        ports = SerialPort.GetPortNames().ToList();

        // set initial values, for quick tests
        dropDown_Ports.AddOptions(ports);
        dropDown_BaudRates.value = 9;
        inputField_Period.text = "500";

        pathTooltip = Instantiate(warningToolTip, canvas.transform);
        pathTooltip.SetActive(false);
    }

    void Update()
    {
        if(isMouseOverPathText && flightRecordsPath != "")
        {
            warningString = "";
            warningString += " " + flightRecordsPath;
            pathTooltip.SetActive(true);
        }
        else
            pathTooltip.SetActive(false);
    }

    public void TrySwitchScene()
    {  
        // CHECK INPUTS ------------------------------------------------------
        warningString = "";
        // ports
        checkCOMPort = false;
        int portIndex = dropDown_Ports.value;
        string portSelected = dropDown_Ports.options[portIndex].text;
        foreach (string port in ports)
        {
            if (portSelected == port)
            {
                checkCOMPort = true;
                break;
            }
        }
        if (checkCOMPort == false)
            warningString += "- COM Port does not match!\n";
        // data period
        checkDataPeriod = true;
        string dataPeriod_Raw = inputField_Period.text;
        foreach (char c in dataPeriod_Raw)
        {
            if(!char.IsDigit(c))
            {
                checkDataPeriod = false;
                warningString += "- Data Period must be integer!\n";
                break;
            }
        }
        if (dataPeriod_Raw == "")
        {
            checkDataPeriod = false;
            warningString += "- Enter Data Period!\n";
        }
        // CHECKS ARE DONE --------------------------------------------------   END

        // CHECK CONDITIONS ----------------------------------------------------
        if(checkCOMPort == false || checkDataPeriod == false)
        {
            // show warning toolbar here, get rid of debugs and put a menu item
            GameObject toolTip = Instantiate(warningToolTip, canvas.transform);
            Destroy(toolTip, 3f);
            return;
        }
        // -----------------------------------------------------------------    END

        // ASSIGN VALUES -------------------------------------------------------
        // port
        dataCOM = portSelected;
        // baud rate 
        int baudRate_Index = dropDown_BaudRates.value;
        string baudRate_Raw = dropDown_BaudRates.options[baudRate_Index].text;
        string baudRate_NoUnit = "";
        foreach(char c in baudRate_Raw)
        {
            if (char.IsDigit(c))
                baudRate_NoUnit += c;
        }
        dataBaudRate = Int32.Parse(baudRate_NoUnit);
        // data period
        dataObtainPeriod = float.Parse(dataPeriod_Raw) / 1000f;
        // -------------------------------------------------------------------  END 

        // START NEXT SCENE
        StartCoroutine(Darken_and_LoadScene());
    }

    IEnumerator Darken_and_LoadScene()
    {
        // animate, wait, load, animate
        SceneTransitions.instance.DarkenGame();
        yield return new WaitForSecondsRealtime(SceneTransitions.instance.animationTime);
        SceneLoader.Load(SceneType.Main);
        SceneTransitions.instance.LightenGame();
    }

    public void OpenExplorer()
    {
        flightRecordsPath = EditorUtility.OpenFolderPanel("Select Record Path", "", "");
        pathText.text = " " + flightRecordsPath;
    }
}
