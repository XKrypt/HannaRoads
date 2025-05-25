using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace HannaRoads
{
    [ExecuteAlways]
    public class ControlPoint : MonoBehaviour
    {
        public Transform root;
        [SerializeField] Vector3 lastPosition;

        public SegmentType segmentType;

        public RSegment rSegment;

        public ReferencePoint referencePoint;

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
            rSegment.Generate();

            if (referencePoint != null)
            {
                referencePoint.UpdateMeshVerts();
            }

            if (segmentType == SegmentType.Start && referencePoint != null)
            {
                if (referencePoint.rSegment != null && referencePoint.previousRSegment != null)
                {
                    Debug.Log("Execute");
                    Transform controlPoint = referencePoint.previousRSegment.controlPoints[1].transform;

                    float distance = Vector3.Distance(referencePoint.transform.position, controlPoint.position);

                    // Direção do ponto que foi movido até o centro (oposto ao que foi movido)
                    Vector3 direction = (referencePoint.transform.position - transform.position).normalized;

                    // Nova posição do ponto oposto (mesma distância, direção oposta)
                    controlPoint.position = referencePoint.transform.position + direction * distance;



                }
            }

            if (segmentType == SegmentType.End && referencePoint != null)
            {
                if (referencePoint.rSegment != null && referencePoint.previousRSegment != null)
                {

                    Transform controlPoint = referencePoint.rSegment.controlPoints[0].transform;

                    float distance = Vector3.Distance(referencePoint.transform.position, controlPoint.position);

                    // Direção do ponto que foi movido até o centro (oposto ao que foi movido)
                    Vector3 direction = (referencePoint.transform.position - transform.position).normalized;

                    // Nova posição do ponto oposto (mesma distância, direção oposta)
                    controlPoint.position = referencePoint.transform.position + direction * distance;

                }
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