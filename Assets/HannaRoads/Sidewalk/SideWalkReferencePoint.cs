using HannaRoads;
using UnityEngine;



[ExecuteInEditMode]
public class SideWalkReferencePoint : MonoBehaviour
{

    public SegmentType segmentType;
    public ConnectionType connectionType;
    public SideWalkSegment sSegment;
    public SideWalkSegment previousSSegment;

    public HannaSidewalk hannaSideWalkEditor;

    [SerializeField] Vector3 lastPosition;

    private void Update()
    {

        UpdateReference();

        lastPosition = transform.position;
    }
    [Range(0, 3)]
    public int interIndex;





    public void UpdateReference()
    {
        if (transform.position != lastPosition)
        {
            if (sSegment != null)
            {
                sSegment.start.position = transform.position;
                segmentType = SegmentType.Start;
            }
            if (previousSSegment != null)
            {
                previousSSegment.end.position = transform.position;
                segmentType = SegmentType.End;

            }
            if (sSegment != null) sSegment.Generate();
            if (previousSSegment != null) previousSSegment.Generate();

        }




        if (transform.position != lastPosition)
        {
            UpdatePositions();
        }


    }

    public void UpdatePositions()
    {
        if (sSegment != null)
        {
            sSegment.start.position = transform.position;
            //rSegment.controlPoints[0].UpdatePositions();
            segmentType = SegmentType.Start;
        }

        // if (sSegment != null)
        // {
        //     foreach (var control in sSegment.controlPoints)
        //     {
        //         control.UpdatePositions();

        //     }
        // }
        // if (previousSSegment)
        // {
        //     foreach (var control in previousSSegment.controlPoints)
        //     {
        //         control.UpdatePositions();

        //     }
        // }



        if (sSegment != null) sSegment.Generate();
        if (previousSSegment != null) previousSSegment.Generate();
    }

    void OnDestroy()
    {
        hannaSideWalkEditor.referencePoints.Remove(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position + (Vector3.up), new Vector3(0.2f, 2, 0.2f));
    }
}