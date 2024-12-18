using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions
{
    public static string ToCSV(this Vector3 vec){
        return $"{vec.x}, {vec.y}, {vec.z}";
    }
}
