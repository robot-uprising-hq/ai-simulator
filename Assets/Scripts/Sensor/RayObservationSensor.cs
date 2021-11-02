using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RayObservationSensor : MonoBehaviour
{
    public float m_MaxAnglePerSide;
    public int m_NumberOfRaysPerSide;
    public float m_MaxDistance;
    // public float m_DistanceRandom;
    public float m_OffsetHeight;
    public float m_CastingSphereSize;
    public float m_FrontCastingSphereSize;

    [Space(10)]
    public List<string> m_DetectableTags;

    [Space(10)]
    public bool m_ShowObservationDebug;
    public bool m_DebugPrintObservationDebug;
    public bool m_DrawObservationRaysDuringPlay;

    private RayCaster[] m_RayCasters;

    private Text m_ObservationDebugText;
    private float[] angles;

    void Awake()
    {
        m_RayCasters = CreateRayCasterSensor(
            m_MaxAnglePerSide,
            m_NumberOfRaysPerSide,
            m_MaxDistance,
            m_OffsetHeight,
            m_CastingSphereSize,
            m_FrontCastingSphereSize,
            m_DetectableTags);
        
        m_ObservationDebugText = GameObject.FindWithTag("debug_text")?.GetComponent<Text>();
    }

    RayCaster[] CreateRayCasterSensor(
        float maxAnglePerSide,
        int numberOfRaysPerSide,
        float maxDistance,
        float m_OffsetHeight,
        float castingSphereSize,
        float frontCastingSphereSize,
        List<string> detectableTags)
    {
        angles = GetAngles(maxAnglePerSide, numberOfRaysPerSide);

        RayCaster[] rayCasters = new RayCaster[angles.Length];
        for (int i = 0; i < angles.Length; i++)
        {
            float sphereSize = Mathf.FloorToInt(angles[i]) == 0 ? frontCastingSphereSize : castingSphereSize;
            rayCasters[i] = new RayCaster(
                gameObject,
                maxDistance,
                m_OffsetHeight,
                angles[i],
                sphereSize,
                detectableTags);
        }
        return rayCasters;
    }

    public float[] GetObservations()
    {
        if (m_RayCasters == null || m_RayCasters.Length < 1) return null;

        float[] allObs = new float[m_RayCasters.Length * (m_DetectableTags.Count + 2)];
        for (int i = 0; i < m_RayCasters.Length; i++)
        {
            float[] sectorObs = m_RayCasters[i].GetObservations(m_DrawObservationRaysDuringPlay);
            Array.Copy(sectorObs, 0, allObs, (i * (m_DetectableTags.Count + 2)), (m_DetectableTags.Count + 2));
        }

        if (m_ShowObservationDebug || m_DebugPrintObservationDebug)
        {
            string observation = "";
            var rayCount = m_RayCasters.Length;
            var sectorObservationCount = m_DetectableTags.Count + 2;
            for (int i = 0; i < rayCount; i++)
            {   
                float[] sectorObs = new float[sectorObservationCount];
                // Array.Copy(sectorObs, 0, allObs, (i * (m_DetectableTags.Count + 2)), (m_DetectableTags.Count + 2));
                Array.Copy(allObs, i * sectorObservationCount, sectorObs, 0, sectorObservationCount);
                var sectorObsStr = String.Format("{0:0.0}: ", angles[i]) + string.Join(" | ", sectorObs);
                observation += sectorObsStr + "\n";
            }
            if (m_ShowObservationDebug && m_ObservationDebugText != null)
                m_ObservationDebugText.text = observation;
            if (m_DebugPrintObservationDebug)
                Debug.Log(observation);
        }

        return allObs;
    }

    public void UpdateCasting(float distance, float distanceRandom, float angleRandom)
    {
        if (m_RayCasters == null) return;
        m_MaxDistance = distance;
        foreach(RayCaster rayCaster in m_RayCasters)
        {
            rayCaster.UpdateCasting(distance, distanceRandom, angleRandom);
        }
    }

    static float[] GetAngles(float maxAnglePerSide, int numberOfRaysPerSide)
    {
        var anglesOut = new float[2 * numberOfRaysPerSide + 1];
        var delta = maxAnglePerSide / numberOfRaysPerSide;

        for (var i = 0; i < numberOfRaysPerSide; i++)
        {
            anglesOut[i] = -maxAnglePerSide + i * delta;
        }
        anglesOut[numberOfRaysPerSide] = 0.0f;
        for (var i = 0; i < numberOfRaysPerSide; i++)
        {
            anglesOut[i + numberOfRaysPerSide + 1] = (i + 1) * delta;
        }
        return anglesOut;
    }

    void OnValidate()
    {
        m_RayCasters = CreateRayCasterSensor(
            m_MaxAnglePerSide,
            m_NumberOfRaysPerSide,
            m_MaxDistance,
            m_OffsetHeight,
            m_CastingSphereSize,
            m_FrontCastingSphereSize,
            m_DetectableTags);
    }

    void OnDrawGizmosSelected()
    {
        if (m_RayCasters == null || m_RayCasters.Length < 1)  return;

        bool inEditMode = Application.isEditor && !Application.isPlaying;
        foreach(RayCaster rayCaster in m_RayCasters)
        {
            rayCaster.DrawCasterGizmos(inEditMode);
        }
    }
}
