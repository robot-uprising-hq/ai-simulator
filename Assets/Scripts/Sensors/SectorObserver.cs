using System;
using System.Collections.Generic;
using UnityEngine;

public class SectorObserver : MonoBehaviour
{
    public List<string> DetextableTags = new List<string>();
    public float[] rayAngles;
    public float rayDistance;
    public float[] GetObservations()
    {
        
        Vector2 origo = new Vector2(transform.position.x, transform.position.z);
        Vector2[] rayVectors = GeometryUtils.CreateRayVectors(transform, rayAngles, rayDistance);
        Vector2[][] segments = GeometryUtils.CreateSegments(rayVectors, origo);
        // GeometryUtils.
        return new float[]{0.0f, 0.1f};
    }


}