using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Metaface.Debug
{
    public class FaceExpressionDemo : MonoBehaviour
    {
        [SerializeField]
        private OVRFaceExpressions faceExpressions;

        [SerializeField]
        private FaceWeightComponent faceWeightPrefab;

        [SerializeField]
        private Transform faceWeightLayout;

        [SerializeField]
        private StudyDataLogger studyDataLogger;

        private List<FaceWeightComponent> components = new List<FaceWeightComponent>();

        //private string filePath = null;
        void Start()
        {
            //create face weights
            BuildDemo();

            // //Make file path
            // filePath = System.IO.Path.Join(
            //     Application.dataPath,
            //     "DataLog",
            //     $"{studyDataLogger.PartID}_faceData_{DateTime.Now.ToString("yyyy-dd-MM-HH-mm")}.csv"
            //     );

            // //Create the file: 
            // if (!System.IO.File.Exists(filePath))
            // {
            //     System.IO.File.Create(filePath).Close();
            // }

            // File.AppendAllText(filePath, GetDataCSVHeader() + "\n");
        }


        private void BuildDemo()
        {
            foreach (OVRFaceExpressions.FaceExpression e in Enum.GetValues(typeof(OVRFaceExpressions.FaceExpression)))
            {
                FaceWeightComponent comp = Instantiate(faceWeightPrefab);
                comp.transform.SetParent(faceWeightLayout, false);
                comp.FaceExpression = e;
                comp.WeightName = e.ToString();
                components.Add(comp);
            }

            //Hide prefab
            faceWeightPrefab.gameObject.SetActive(false);
        }

        void Update()
        {
            //Do update
            foreach (FaceWeightComponent comp in components)
            {
                float weight;
                if (faceExpressions.TryGetFaceExpressionWeight(comp.FaceExpression, out weight))
                {
                    if (!comp.gameObject.activeInHierarchy)
                        comp.gameObject.SetActive(true);
                    comp.WeightValue = weight;
                }
                else
                {
                    if (comp.gameObject.activeInHierarchy)
                        comp.gameObject.SetActive(false);  //testing can turn back to true
                }
            }
            //File.AppendAllText(filePath, GetDataCSV() + "\n");
        }


        internal string GetDataCSV()
        {
            var ret = $"";
            foreach (OVRFaceExpressions.FaceExpression e in Enum.GetValues(typeof(OVRFaceExpressions.FaceExpression)))
            {
                float weight;
                if (faceExpressions.TryGetFaceExpressionWeight(e, out weight))
                {
                    ret += $"{weight},";
                }
                else
                {
                    ret += "_,";
                }
            }
            return ret.TrimEnd(',');
        }

        public string GetDataCSVHeader()
        {
            var header = "";
            foreach (OVRFaceExpressions.FaceExpression e in Enum.GetValues(typeof(OVRFaceExpressions.FaceExpression)))
            {
                header += $"{e},";
            }
            return header.TrimEnd(',');
        }
    }
}