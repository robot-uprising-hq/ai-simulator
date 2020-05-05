using System;
using System.Collections.Generic;
using UnityEngine;

public static class GeometryUtils
{
    public static Vector2[] CreateRayVectors(Transform transform, float[] rayAngles, float rayDistance)
    {
        Vector3 forwardVec = transform.forward;
        Vector2[] rayVectors = new Vector2[rayAngles.Length];
        for (var i = 0; i < rayAngles.Length; i++)
        {
            Vector3 dirVec = new Vector3(forwardVec.x, forwardVec.y, forwardVec.z);
            Vector3 newVec =  Quaternion.Euler(0, rayAngles[i], 0) * dirVec * rayDistance;

            rayVectors[i] = new Vector2(newVec.x, newVec.z);
            Debug.DrawRay(transform.position, newVec, Color.yellow, 0.1f, true);
        }

        return rayVectors;
    }

    public static Vector2[][] CreateSegments(Vector2[] rayVectors, Vector2 origo)
    {
        Vector2[][] segments = new Vector2[rayVectors.Length - 1][];
        for (var i = 0; i < rayVectors.Length - 1; i++)
        {
            Vector2[] segmentArray = new Vector2[3];
            segmentArray[0] = origo;
            segmentArray[1] = origo + rayVectors[i];
            segmentArray[2] = origo + rayVectors[i+1];
            segments[i] = segmentArray;
        }
        if (Application.isEditor)
        {
            DrawSegments(segments);
        }
        return segments;
    }

            /// <summary>
    /// Determines if the given point is inside the polygon
    /// </summary>
    /// <param name="polygon">the vertices of polygon</param>
    /// <param name="testPoint">the given point</param>
    /// <returns>true if the point is inside the polygon; otherwise, false</returns>
    public static bool IsPointInPolygon4(Vector2[] polygon, Vector2 testPoint)
    {
        bool result = false;
        int j = polygon.Length - 1;
        for (int i = 0; i < polygon.Length; i++)
        {
            if (polygon[i].y < testPoint.y && polygon[j].y >= testPoint.y || polygon[j].y < testPoint.y && polygon[i].y >= testPoint.y)
            {
                if (polygon[i].x + (testPoint.y - polygon[i].y) / (polygon[j].y - polygon[i].y) * (polygon[j].x - polygon[i].x) < testPoint.x)
                {
                    result = !result;
                }
            }
            j = i;
        }
        return result;
    }

    public static void DrawSegments(Vector2[][] segments)
    {
        foreach (Vector2[] segment in segments)
        {
            Vector3 start = new Vector3(segment[0].x, 0.0f, segment[0].y);
            Vector3 endOne = new Vector3(segment[1].x, 0.0f, segment[1].y);
            Vector3 endTwo = new Vector3(segment[2].x, 0.0f, segment[2].y);
            Debug.DrawRay(start, endOne, Color.green, 0.1f, true);
            Debug.DrawRay(start, endTwo, Color.green, 0.1f, true);
            Debug.DrawLine(endTwo, endOne, Color.green, 0.1f, true);
        }
    }
}
