using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Metaface.Debug
{
    public class DataLog_FaceData : MonoBehaviour
    {
        [SerializeField]
        private OVRFaceExpressions faceExpressions;

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