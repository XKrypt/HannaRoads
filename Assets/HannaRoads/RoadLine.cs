using UnityEngine;
using HannaRoads;


namespace HannaRoads
{


    [ExecuteInEditMode]
    public class RoadLine : MonoBehaviour
    {
        public AnimationCurve widthProfile;
        public AnimationCurve verticalProfile;
        public float start;
        public float end;


        public RSegment rSegment;

        public float horizontalOffset;

        public float width = 1;
        public float verticalProfileMultiplayer = 0;
        public float verticalOffset = 0;
        public int detailLevel = 5;
        public int sliceResolution = 2;

        public Mesh mesh;
        public MeshRenderer meshRenderer;


        void OnDestroy()
        {
            if (rSegment != null)
            {
                rSegment.roadLines.Remove(this);
            }
        }
    }


}