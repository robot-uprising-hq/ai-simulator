using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectorPerceptionSensor : MonoBehaviour
{
    public List<float> m_Angles = new List<float>();
    public float m_MaxDistance;
    public float m_OffsetHeight;
    [Space(10)]
    public List<string> m_DetectableTags;

    private SectorCaster[] m_SectorCasters;

    public int SectorObservationCount
    {
        get => m_DetectableTags.Count + 2;
    }

    public int SectorCount
    {
        get => m_SectorCasters.Length;
    }

    void Awake()
    {
        m_SectorCasters = CreateSectorCaster(m_Angles, m_MaxDistance, m_OffsetHeight, m_DetectableTags);
    }

    SectorCaster[] CreateSectorCaster(List<float> angles, float maxDistance, float m_OffsetHeight, List<string> detectableTags)
    {
        if (angles.Count < 1) return null;

        SectorCaster[] sectorCasters = new SectorCaster[angles.Count - 1];
        for (int i = 0; i < angles.Count -1; i++)
        {
            sectorCasters[i] = new SectorCaster(transform, maxDistance, m_OffsetHeight, angles[i], angles[i+1], detectableTags);
        }
        return sectorCasters;
    }

    public float[] GetObservations()
    {
        if (m_SectorCasters == null || m_SectorCasters.Length < 1) return null;

        float[] allObs = new float[m_SectorCasters.Length * (m_DetectableTags.Count + 2)];
        for (int i = 0; i < m_SectorCasters.Length; i++)
        {
            float[] sectorObs = m_SectorCasters[i].GetObservations();
            Array.Copy(sectorObs, 0, allObs, (i * (m_DetectableTags.Count + 2)), (m_DetectableTags.Count + 2));
        }

        return allObs;
    }

    public void UpdateCastingDistance(float distance)
    {
        m_MaxDistance = distance;
        foreach(SectorCaster sectorCaster in m_SectorCasters)
        {
            sectorCaster.UpdateCastingDistance(distance);
        }
    }

    void OnValidate()
    {
        m_SectorCasters = CreateSectorCaster(m_Angles, m_MaxDistance, m_OffsetHeight, m_DetectableTags);
    }

    void OnDrawGizmos()
    {
        if (m_SectorCasters == null || m_SectorCasters.Length < 1)  return;

        foreach(SectorCaster sectorCaster in m_SectorCasters)
        {
            sectorCaster.DrawCasterGizmos();
        }
    }
}