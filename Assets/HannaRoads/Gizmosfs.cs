using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gizmosfs 
{
        public static void DrawWireCircle(Vector3 pos, Quaternion rot, float radius, int detailLevel = 32){
        Vector3[] points3D = new Vector3[detailLevel];

        for (int i = 0; i < detailLevel; i++)
        {
            float t = i / (float)detailLevel;
            float angRad = t * MathFunc.TAU;

            Vector2 point2D = MathFunc.GetUnitVectorByAngle(angRad) * radius;

            
            points3D[i] = pos + rot * point2D ;
        }

        for (int i = 0; i < detailLevel - 1; i++){
            Gizmos.DrawLine(points3D[i],points3D[i+1]);
        }
         Gizmos.DrawLine(points3D[detailLevel - 1],points3D[0]);

    }
}
