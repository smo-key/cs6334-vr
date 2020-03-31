using UnityEngine;
using UnityEditor;

public static class MathUtil
{
    public static float Distance(Vector3 a, Vector3 b)
    {
        return Mathf.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y) + (b.z - a.z) * (b.z - a.z));
    }

    public static float Length(Vector3 a)
    {
        return Distance(a, Vector3.zero);
    }

    public static Vector3 ClosestPointOnCircle(Vector3 point, Vector3 circleCenter, float circleRadius)
    {
        var diff = point - circleCenter;
        return diff/Length(diff)*circleRadius;
    }
}