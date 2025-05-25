using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BevelTest : MonoBehaviour
{
    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 pointC;
    public Vector3 m_intersection;
    public float distortion;
    public int detailLevel;

    public float amount;

    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(pointA, 0.05f);
        Gizmos.DrawSphere(pointB, 0.05f);
        Gizmos.DrawSphere(pointC, 0.05f);


        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawLine(pointC, pointB);


        TPoint points = GetPoint(amount);

        Gizmos.DrawSphere(points.aToB, 0.05f);
        Gizmos.DrawSphere(points.cToB, 0.05f);

        Vector3 AC = points.cToB + points.aToB;
        Vector3 intersection = CalculateFourthPoint(points.cToB,points.aToB);

        intersection = intersection + (Vector3.right * distortion);
        

        m_intersection = intersection;

        Vector3 directionAToB = (points.aToB - intersection);
        Vector3 directionCToB = (points.cToB - intersection);




        Gizmos.DrawLine(intersection, intersection + directionAToB);
        Gizmos.DrawLine(intersection, intersection + directionCToB);
        Gizmos.DrawSphere(intersection, 0.05f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(CalculateFourthPoint(pointC, pointA), 0.05f);
        Gizmos.color = Color.white;
        

        for (int i = 0; i < detailLevel; i++)
        {

            float t = i / (float)detailLevel;

            float angle = Vector3.Angle(directionAToB, directionCToB) * t;
       

            float distance = Mathf.Lerp(Vector3.Distance(intersection, points.aToB), Vector3.Distance(intersection, points.cToB), t);

            Vector2 arcPosition = new Vector2(Mathf.Cos(angle * (Mathf.PI / 180)), Mathf.Sin(angle * (Mathf.PI / 180))) * distance;
            Gizmos.DrawSphere((Vector3)(arcPosition) + intersection, 0.02f);




        }


    }


    Vector3 CalculateFourthPoint(Vector3 A, Vector3 C)
    {
        // x coordinate of D is the same as x coordinate of A
        // y coordinate of D is the same as y coordinate of C
        return new Vector3(A.x, C.y, A.z);
    }

    void DrawShape(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        Debug.DrawLine(A, B, Color.red, 100f);
        Debug.DrawLine(B, C, Color.red, 100f);
        Debug.DrawLine(C, D, Color.red, 100f);
        Debug.DrawLine(D, A, Color.red, 100f);
    }




    TPoint GetPoint(float t)
    {

        return new TPoint()
        {
            aToB = Vector3.Lerp(pointA, pointB, t),
            cToB = Vector3.Lerp(pointC, pointB, t),
        };
    }





}



struct TPoint
{
    public Vector3 aToB;
    public Vector3 cToB;
}