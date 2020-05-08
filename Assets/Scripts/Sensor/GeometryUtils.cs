using System.Collections;
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
        return segments;
    }

    public static Vector2[][] SectorVertices(Transform origo, float[] rayAngles, float rayDistance)
    {
        Vector2[] rayVectors = CreateRayVectors(origo, rayAngles, rayDistance);
        Vector2 origoVec2 = new Vector2(origo.position.x, origo.position.z);
        Vector2[][] sectors = CreateSegments(rayVectors, origoVec2);
        return sectors;
    }
}