using HannaRoads;
using UnityEngine;


[ExecuteInEditMode]
public class RoadObject : MonoBehaviour
{
    public float roadPosition;
    public float roadHorizontalOffset;
    public float heightOffset;
    public bool alignWithRoad;

    public RSegment rSegment;

    void Update()
    {
        if (rSegment != null)
        {

            OrientedPoint pos = rSegment.GetBezierPointGlobal(roadPosition);
            transform.position = pos.LocalSpace(Vector3.right * roadHorizontalOffset + (Vector3.up * heightOffset));

            if (alignWithRoad)
            {
                transform.rotation = pos.rot;
            }

        }
    }
}