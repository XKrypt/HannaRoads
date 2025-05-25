using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatGPTBevel : MonoBehaviour
{
    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 center;
    public float radius = 1.0f;
    public float shape = 0.0f; // -1 (côncavo), 0 (reto), 1 (convexo)
    public int segments = 10;
    public float width = 0.5f; // Width of the bevel
    public bool widthAsPercentage = false; // If true, width is treated as a percentage


    void OnDrawGizmos()
    {

        center = (pointA + pointB) * shape;
        Gizmos.DrawSphere(pointA, 0.05f);
        Gizmos.DrawSphere(pointB, 0.05f);
        Gizmos.DrawSphere(center, 0.05f);

        Vector3[] bevelPoints = CalculateBevelPoints(pointA, pointB, center,width,widthAsPercentage,shape, segments);
        DrawBevel(bevelPoints);
    }

   Vector3[] CalculateBevelPoints(Vector3 A, Vector3 B, Vector3 C, float width, bool widthAsPercentage, float shape, int segments)
    {
        // Calculate vectors
        Vector3 AB = (B - A).normalized;
        Vector3 AC = (C - A).normalized;

        // Calculate the effective width
        float actualWidth = widthAsPercentage ? width * Vector3.Distance(A, C) : width;

        // Calculate the tangent points based on width
        Vector3 tangentA = A + AC * actualWidth;
        Vector3 tangentB = B + AB * actualWidth;

        // Determine the direction for control point (the bisector)
        Vector3 bisector = (AC + AB).normalized;

        // Shape adjustment: control how far out the control point is based on shape
        Vector3 controlPoint = C + bisector * actualWidth * Mathf.Lerp(0.5f, 2.0f, shape);

        // Create the bezier points
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            points[i] = CalculateQuadraticBezierPoint(t, tangentA, controlPoint, tangentB);
        }

        return points;
    }

    Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 point = (uu * p0) + (2 * u * t * p1) + (tt * p2);
        return point;
    }

    void DrawBevel(Vector3[] points)
    {
        for (int i = 0; i < points.Length - 1; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
    }
}
