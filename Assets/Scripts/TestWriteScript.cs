using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Photon.Realtime;
using System;

public class TestWriteScript : MonoBehaviour
{
    private string filePath;

    private void Start()
    {
        filePath = GetFilePath();
    }


    private void addRecord(float time, string filePath)
    {
        try
        {
           // if (!startWriting)
          //  {
          //      using (StreamWriter file = new StreamWriter(@filePath, false))
          //      {
         //           file.WriteLine("UserID");
          //      }
          //      startWriting = true;
          //  }
            
                using (StreamWriter file = new StreamWriter(@filePath, true))
                {
                    file.WriteLine("HHHHH");
                }
            
        }
        catch (Exception ex)
        {
            Debug.Log("Something went wrong! Error: " + ex.Message);
        }
    }


    string GetFilePath()
    {
        return Application.persistentDataPath + "/" + "_" + "testETRIdata" + ".csv";
    }

    
    public void pppp()
    {
        addRecord(Time.time, filePath);
    }
}
