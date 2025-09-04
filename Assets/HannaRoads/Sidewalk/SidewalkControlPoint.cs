using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace HannaRoads
{
    [ExecuteAlways]
    public class SidewalkControlPoint : MonoBehaviour
    {
        public Transform root;
        [SerializeField] Vector3 lastPosition;

        public SegmentType segmentType;


        bool isSideWalk;
        public SideWalkSegment sSegment;

        public SideWalkReferencePoint referencePoint;

        private void Update()
        {
            if (Application.isPlaying) return;
            if (lastPosition != transform.position)
            {
                UpdatePositions();
            }
            lastPosition = transform.position;
        }

        public void UpdatePositions()
        {
            sSegment.Generate();

            referencePoint = segmentType == SegmentType.Start ? sSegment.startRef : sSegment.endRef;



            if (referencePoint.sSegment != null)
            {
                referencePoint.sSegment.Generate();
            }
            if (referencePoint.previousSSegment != null)
            {

                referencePoint.previousSSegment.Generate();

            }

        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, root.position);

            // if (root == null) return;
            // Gizmos.DrawCube(transform.position, Vector3.one * 0.2f);
            // Gizmos.color = Color.white;

        }
    }
}