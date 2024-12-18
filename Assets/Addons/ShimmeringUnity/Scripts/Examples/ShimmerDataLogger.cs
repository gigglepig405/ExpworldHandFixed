using System.Collections.Generic;
using ShimmerAPI;
using UnityEngine;
using System.IO;
using System;

namespace ShimmeringUnity
{
    /// <summary>
    /// Example of logging data from a shimmer device
    /// </summary>
    /// 

    public class ShimmerDataLogger : MonoBehaviour
    {

        [SerializeField]
        private StudyDataLogger studyDataLogger;
        [SerializeField]
        [Tooltip("Reference to the shimmer device.")]
        private ShimmerDevice shimmerDevice;

        [System.Serializable]
        public class Signal
        {
            [SerializeField]
            [Tooltip("The signal's name. More info in the signals section of the readme.")]
            private ShimmerConfig.SignalName name;

            public ShimmerConfig.SignalName Name => name;

            [SerializeField]
            [Tooltip("The signal's format.")]
            private ShimmerConfig.SignalFormat format;

            public ShimmerConfig.SignalFormat Format => format;

            [SerializeField]
            [Tooltip("The units the signal's value is displayed in, set to \"Automatic\" for default.")]
            private ShimmerConfig.SignalUnits unit;

            public ShimmerConfig.SignalUnits Unit => unit;

            [SerializeField]
            [Tooltip("The value output of this signal (only for debug purposes).")]
            private string value = null;

            public string Value
            {
                set => this.value = value;
                get => value;
            }
        }

        [SerializeField]
        [Tooltip("List of signals to record from this device.")]
        private List<Signal> signals = new List<Signal>();

        public List<Signal> Signals => signals;

        private bool isStreaming = false;

        private string filePathSelected, filePathAll;

        private void OnEnable()
        {
            //Listen to the data recieved event when enabled
            shimmerDevice?.OnDataRecieved.AddListener(OnDataRecieved);
            shimmerDevice?.OnStateChanged.AddListener(OnStateChanged);
        }

        private void OnDisable()
        {
            //Stop listening to the data recieved event when disabled
            shimmerDevice?.OnDataRecieved.RemoveListener(OnDataRecieved);
        }

        /// <summary>
        /// Event listener for the shimmer device's data recieved event
        /// </summary>
        /// <param name="device"></param>
        /// <param name="objectCluster"></param>
        private void OnDataRecieved(ShimmerDevice device, ObjectCluster objectCluster)
        {
            string selectedCsvData = DateTime.Now.ToString("yyyy-dd-MM-HH-mm") + ",";
            string allCsvData = DateTime.Now.ToString("yyyy-dd-MM-HH-mm") + ",";

            foreach (var signal in signals)
            {
                //Get the data
                SensorData data = signal.Unit == ShimmerConfig.SignalUnits.Automatic ?
                    objectCluster.GetData(
                        ShimmerConfig.NAME_DICT[signal.Name],
                        ShimmerConfig.FORMAT_DICT[signal.Format]) :
                    objectCluster.GetData(
                        ShimmerConfig.NAME_DICT[signal.Name],
                        ShimmerConfig.FORMAT_DICT[signal.Format],
                        ShimmerConfig.UNIT_DICT[signal.Unit]);

                //If data is null, early out
                if (data == null)
                {
                    signal.Value = "NULL";
                }
                else
                {
                    //Write data back into the signal for debugging
                    signal.Value = $"{data.Data} {data.Unit}";
                }
                selectedCsvData += $"{signal.Value},";
            }

            foreach (string name in ShimmerConfig.NAME_DICT.Values)
            {
                SensorData data = objectCluster.GetData(name, "CAL");
                string val = "";
                if (data == null)
                {
                    val = "NULL";
                }
                else
                {
                    //Write data back into the signal for debugging
                    val = $"{data.Data} {data.Unit}";
                }
                allCsvData += $"{val},";
            }

            selectedCsvData = selectedCsvData.TrimEnd(',') + "\n";
            allCsvData = allCsvData.TrimEnd(',') + "\n";

            //This is where you can do something with the data...
            if (isStreaming && File.Exists(filePathSelected))
            {
                File.AppendAllText(filePathSelected, selectedCsvData);
            }

            if (isStreaming && File.Exists(filePathAll))
            {
                File.AppendAllText(filePathAll, allCsvData);
            }
        }

        private void OnStateChanged(ShimmerDevice device, ShimmerDevice.State state)
        {
            Debug.Log("The device has changed state");
            if (state == ShimmerDevice.State.Streaming)
            {
                CreateDataFile();
                RecordAllData();
                isStreaming = true;
            }
            else
            {
                isStreaming = false;
                filePathSelected = "";
                filePathAll = "";
            }
        }

        public void CreateDataFile()
        {
            filePathSelected = $"{Application.dataPath}/DataLog/{studyDataLogger.PartID}_shimmer_data_{DateTime.Now.ToString("yyyy-dd-MM-HH-mm")}.csv";

            //Create and close the file
            File.Create(filePathSelected).Close();

            //Append header
            var header = "System Time (PC),";
            foreach (Signal signal in signals)
            {
                header += $"{ShimmerConfig.NAME_DICT[signal.Name]},";
            }
            header = header.TrimEnd(',') + "\n";
            File.AppendAllText(filePathSelected, header);
        }


        public void RecordAllData()
        {
            filePathAll = $"{Application.dataPath}/DataLog/shimmer_AllData_{DateTime.Now.ToString("yyyy-dd-MM-HH-mm")}.csv";

            //Create and close the file
            File.Create(filePathAll).Close();

            //Append header
            var header = "System Time (PC),";
            foreach (string name in ShimmerConfig.NAME_DICT.Values)
            {
                header += $"{name},";
            }
            header = header.TrimEnd(',') + "\n";
            File.AppendAllText(filePathAll, header);

        }

        public string GetDataCSV()
        {
            string ret = "";
            foreach (var signal in signals)
            {
                ret += $"{(string.IsNullOrWhiteSpace(signal.Value) ? "_" : signal.Value)},";
            }
            return ret.TrimEnd(',');
        }

        public string GetDataCSVHeader()
        {
            string ret = "";
            foreach (var signal in signals)
            {
                ret += $"{signal.Name},";
            }
            return ret.TrimEnd(',');
        }

    }
}
