
using UnityEngine;


class MathFunc
{
    public const  float TAU = Mathf.PI * 2;
    public static Vector2 GetUnitVectorByAngle(float angRad)
    {

        return new Vector2(
                Mathf.Cos(angRad),
                Mathf.Sin(angRad)
            );
    }
}