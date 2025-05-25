using UnityEngine;

public struct OrientedPoint
{
    public Vector3 pos;
    public Quaternion rot;


    public Vector3 LocalToWorldPos(Vector3 localSpace)
    {
        return pos + rot * localSpace;
    }
    public Vector3 LocalSpace(Vector3 localSpace)
    {
        return pos + rot * localSpace;
    }
    public Vector3 LocalToWorldVector(Vector3 localSpace)
    {
        return rot * localSpace;
    }
}