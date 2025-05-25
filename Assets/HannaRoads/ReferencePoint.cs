using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HannaRoads
{
    [ExecuteInEditMode]

    public class ReferencePoint : MonoBehaviour
    {
        public SegmentType segmentType;
        public ConnectionType connectionType;
        public RSegment rSegment;
        public RSegment previousRSegment;


        public RSegment connectedRSegment;


        public bool connected;


        public HannaRoad hannaRoad;



        public HannaIntersection intersectionAttachment;

        public void SetType(SegmentType segment)
        {
            segmentType = segment;
        }
        public void SetRSegment(RSegment segment)
        {
            rSegment = segment;
        }

       [SerializeField] Vector3 lastPosition;
       [SerializeField] int lastInterIndex;

        private void Update()
        {

            UpdateReference();

            lastPosition = transform.position;
        }
        [Range(0, 3)]
        public int interIndex;


        public void CreateIntersection()
        {
            HannaIntersection intersection = new GameObject().AddComponent<HannaIntersection>();

            intersection.extrusionSize = 1f;
            intersection.size = 0.5f;
            intersection.shape = 0f;

            intersection.transform.position = transform.position;

            intersection.Generate();


            intersectionAttachment = intersection;

            intersection.transform.SetParent(transform.parent);
            intersection.gameObject.name = "Intersection." + name.Last();

            if (segmentType == SegmentType.Start)
            {
                rSegment.startIntersection = intersection;
            }
            else
            {
                previousRSegment.endIntersection = intersection;
            }

            intersection.hannaRoad = hannaRoad;
            hannaRoad.activeIntersection = intersection;
        }




        public void UpdateReference()
        {
            if (intersectionAttachment != null)
            {
                transform.position = intersectionAttachment.transform.TransformPoint(intersectionAttachment.intersectionPoints[interIndex].middlePoint);

                if (transform.position != lastPosition || lastInterIndex != interIndex)
                {
                    if (rSegment != null)
                    {
                        rSegment.start.position = transform.position;
                        segmentType = SegmentType.Start;
                    }
                    if (previousRSegment != null)
                    {
                        previousRSegment.end.position = transform.position;
                        segmentType = SegmentType.End;

                    }
                    if (rSegment != null) rSegment.Generate();
                    if (previousRSegment != null) previousRSegment.Generate();

                    lastInterIndex = interIndex;

                }
                return;
            }



            if (transform.position != lastPosition)
            {
                UpdatePositions();
            }


        }

        public void UpdatePositions()
        {
            if (rSegment != null)
            {
                rSegment.start.position = transform.position;
                //rSegment.controlPoints[0].UpdatePositions();
                segmentType = SegmentType.Start;
            }
            if (previousRSegment != null)
            {
                if (connected)
                {
                    previousRSegment.start.position = transform.position;
                    previousRSegment.controlPoints[0].UpdatePositions();
                    segmentType = SegmentType.Start;
                }
                else
                {
                    previousRSegment.end.position = transform.position;
                    segmentType = SegmentType.End;
                    //previousRSegment.controlPoints[1].UpdatePositions();
                }

            }
            if (rSegment != null) rSegment.Generate();
            if (previousRSegment != null || connected) previousRSegment.Generate();
        }

        void OnDestroy()
        {
            hannaRoad.referencePoints.Remove(this);
        }

        private void OnDrawGizmos()
        {

        }

        public void UpdateMeshVerts()
        {


            // if (previousRSegment != null && rSegment != null)
            // {
            //     rSegment.controlPoints[0].referencePoint = this;
            //     previousRSegment.controlPoints[1].referencePoint = this;


            //     if (connected)
            //     {
            //         Vector3[] vertices = rSegment.GetFirstVertices();

            //         previousRSegment.SetFirstVertices(vertices[1], vertices[0]);
            //         return;
            //     }

            //     Vector3[] verts = rSegment.GetFirstVertices();

            //     previousRSegment.SetLastVertices(verts[1], verts[0]);
            // }


            //TODO Fazer umm lerp da direita pra esquerda
            //TODO Outro lerp e controles da curvatura da estrada no eixo y para ficar no eixo 0 Obs: tanto para o inicio, quanto para o final
            //TODO Fazer o mesmo para a conexão de estrada que tenha curvas no eixo y diferentes
            // curvatura y da estrada atual -> curvatura y da próxima estrada 
            if (intersectionAttachment != null)
            {

                Vector3 vertA = intersectionAttachment.intersectionPoints[interIndex].leftPoint;
                Vector3 vertB = intersectionAttachment.intersectionPoints[interIndex].rightPoint;
                if (segmentType == SegmentType.Start)
                {

                    Vector3[] verts = new Vector3[rSegment.sliceResolution];
                    for (int i = 0; i < verts.Length; i++)
                    {
                        float t = (float)i / (verts.Length - 1);

                        verts[i] = intersectionAttachment.transform.TransformPoint(Vector3.Lerp(vertB, vertA, t));

                    }

                    rSegment.SetFirstVertices(verts);
                }
                else
                {
                    Vector3[] verts = new Vector3[previousRSegment.sliceResolution];
                    for (int i = 0; i < verts.Length; i++)
                    {
                        float t = (float)i / (verts.Length - 1);
                      
                        verts[i] = intersectionAttachment.transform.TransformPoint(Vector3.Lerp(vertA, vertB, t));

                    }
                    previousRSegment.SetLastVertices(verts);
                }



            }
        }
    }
    public enum SegmentType
    {
        Start,
        End,
    }

    public enum ConnectionType
    {
        ConnectedStart,
        ConnectedEnd
    }
}

