using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using Metaface.Debug;
using Metaface.Utilities;
using ShimmeringUnity;
using UnityEngine;


public class StudyDataLogger : MonoBehaviour
{
    [SerializeField]
    private ShimmerDataLogger shimmerDataLogger;
    [SerializeField]
    private ShimmerHeartRateMonitor shimmerHeartRateMonitor;

    public TaskManagerScript tm;

    MidEyeGazeHelper meg;
    DataLog_FaceData faceData;

    public string PartID = "t";


    public bool isLogging = false;
    private string filePath;

    public bool skipLog;
    public bool useShimmer;


    public void InitiateLogger()
    {
        //DO LOG 
        if (!skipLog)
        {
            //FOR Gaze Focus OBJ init
            meg = GameObject.Find("EyeGazeDebugger").GetComponent<MidEyeGazeHelper>();
            //FOR Self Face Data
            faceData = GameObject.Find("SyncAvatarLocal").GetComponent<DataLog_FaceData>();

            CreateParticipant();
            isLogging = true;
        }

        //DO NOT LOG
        else
        {
            //For empathic portal
            /**
            if (GameObject.Find("PortalA") != null) //runs EmpathicPortal
            {
                GameObject.Find("Fundamentals").GetComponent<PortalGazeManager>().initGaze(); //start portal gaze manager

                //stitch portal window to user's head..
                GameObject.Find("PortalA").transform.parent = GameObject.Find("MainPlayerHeadRefPoint").transform;
                GameObject.Find("PortalA").transform.localPosition = new Vector3(0, 0, -0.2f);
                GameObject.Find("PortalA").transform.localEulerAngles = Vector3.zero;
            }
            */
        }

        //disable visual indicator to show DATA LOGGING init HAS DONE
        this.transform.GetChild(0).gameObject.SetActive(false); 
    }

    void CreateParticipant()
    {
        var fileName = PartID.Split('-')[0] + "_" + PartID.Split('-')[1];
        var participantFolder = System.IO.Path.Join(Application.dataPath, "DataLog", fileName);

        if (!System.IO.Directory.Exists(participantFolder))
        {
            System.IO.Directory.CreateDirectory(participantFolder);
        }

        filePath = System.IO.Path.Join(
            participantFolder,
            $"{fileName}l_study_data_{DateTime.Now.ToString("yyyy-dd-MM-HH-mm")}.csv"
            );

        int fileAppend = 0;
        while (System.IO.File.Exists(filePath))
        {
            fileAppend++;
            filePath = System.IO.Path.Join(
            Application.dataPath,
            "DataLog",
            $"{fileName}l_study_data_{DateTime.Now.ToString("yyyy-dd-MM-HH-mm")}_{fileAppend}.csv"
            );
        }

        System.IO.File.Create(filePath).Close();

        //Build header csv
        StringBuilder strbldr = new StringBuilder();

        strbldr.Append("ID,TIMESTAMP,PAIR,CONDITION,");

        //Get eye data header
        strbldr.Append("GAZE_FOCUS");
        strbldr.Append(",");

        //face header
        strbldr.Append(faceData.GetDataCSVHeader());

        if (useShimmer)
        {
            strbldr.Append(",");
            //Get HR header
            strbldr.Append(shimmerHeartRateMonitor.GetDataCSVHeader());
            strbldr.Append(",");
            //get shimmer header
            strbldr.Append(shimmerDataLogger.GetDataCSVHeader());
        }

        strbldr.Append("\n");

        System.IO.File.AppendAllText(filePath, strbldr.ToString());
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (isLogging && !skipLog)
        {
            //build the current frame csv row
            StringBuilder strbldr = new StringBuilder();

            //Get participantID, system time and condition
            //strbldr.Append($"{PartID.Split('-')[0]},{DateTime.Now.ToString("yyyy-dd-MM-HH-mm:ss:ffff")},{PartID.Split('-')[1]},{tm.cond},");

            //Get eye data
            strbldr.Append(meg.focusOBJ);
            strbldr.Append(",");

            //face
            strbldr.Append(faceData.GetDataCSV());

            if (useShimmer)
            {
                strbldr.Append(",");
                //Get HR data 
                strbldr.Append(shimmerHeartRateMonitor.GetDataCSV());
                strbldr.Append(",");
                //get shimmer data logger CSV row:
                strbldr.Append(shimmerDataLogger.GetDataCSV());
            }
            strbldr.Append("\n");

            //Write to the file
            System.IO.File.AppendAllText(filePath, strbldr.ToString());
        }
    }
}
