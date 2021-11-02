using System;
using System.Collections.Generic;
using UnityEngine;

public struct PerceptionOutput
{
    public float Distance;
    public GameObject hitGo;
}

public class RayCaster
{
    private Color m_GizmoColor = Color.green;
    private Color m_GizmoFrontColor = Color.yellow;
    GameObject m_Go;
    Transform m_RobotTrans;
    float m_OffsetHeight;
    float m_CastingDistance;
    float m_CastingDistanceRandom = 0.0f;
    float m_CastSphereSize;

    float m_CastBoxWidth = 0.01f;
    float m_CastBoxDepth = 0.01f;

    float m_AngleDeg;
    float m_AngleDegRandom = 0.0f;
    float m_CurrentAngle;
    List<string> m_DetectableTags;

    RaycastHit m_Hit;
    Vector3 m_CastingDir;
    bool m_HitDetect;
    PerceptionOutput output;
    bool rayAdded = false;
    LineRenderer rayLine;

    public RayCaster(
        GameObject go,
        float castingDistance,
        float offsetHeight,
        float angle,
        float castSphereSize,
        List<string> detectableTags)
    {
        m_Go = go;
        m_RobotTrans = go.transform;
        m_CastingDistance = castingDistance;
        m_OffsetHeight = offsetHeight;
        m_AngleDeg = angle;
        m_CastSphereSize = castSphereSize;
        m_DetectableTags = detectableTags;
    }

    public void UpdateCasting(float castingDistance, float distanceRandom, float angleRandom)
    {
        m_CastingDistance = castingDistance;
        m_CastingDistanceRandom = distanceRandom;
        m_AngleDegRandom = angleRandom;
    }

    public float[] GetObservations(bool drawRays)
    {
        float[] observations = new float[m_DetectableTags.Count + 2];
        output = Cast();
        if (drawRays)
            // if (rayAdded == false)
            // {
            //     rayAdded = true;
            //     Debug.Log("=== AddLineRenderer");
            //     AddLineRenderer();
            // }
            DrawObservationRays(output);
        
        if (output.hitGo != null)
        {
            // Find the index of the tag of the object that was hit.
            for (var i = 0; i < m_DetectableTags.Count; i++)
            {
                if (output.hitGo.CompareTag(m_DetectableTags[i]))
                {
                    observations[i] = 1.0f;
                    float distance = output.Distance / m_CastingDistance;
                    distance = distance * UnityEngine.Random.Range(1.0f - m_CastingDistanceRandom, 1.0f);
                    observations[observations.Length - 1] = distance;
                    break;
                }
            }
        }
        else
        {
            observations[observations.Length - 2] = 1.0f;
        }

        return observations;
    }
 
    public PerceptionOutput Cast()
    {
        m_CurrentAngle = m_AngleDeg + UnityEngine.Random.Range(-m_AngleDegRandom, m_AngleDegRandom);
        m_CastingDir = Quaternion.Euler(0, m_CurrentAngle, 0) * m_RobotTrans.forward;

        m_HitDetect = Physics.BoxCast(
            m_RobotTrans.position + new Vector3(0.0f, m_OffsetHeight, 0.0f),
            new Vector3(m_CastSphereSize, m_CastBoxWidth, m_CastBoxDepth),
            m_CastingDir,
            out m_Hit,
            Quaternion.identity,
            m_CastingDistance);

        output = new PerceptionOutput();
        if (m_HitDetect)
        {
            float hitDistance = Vector3.Distance(m_RobotTrans.position + new Vector3(0.0f, m_OffsetHeight, 0.0f), m_Hit.point);
            output.Distance = hitDistance;
            output.hitGo = m_Hit.collider.gameObject;
        }
        else
        {
            output.Distance = 0.0f;
            output.hitGo = null;
        }

        return output;
    }

    public void DrawCasterGizmos(bool inEditMode)
    {
        // TODO: Draw the actual outputs which were created a moment ago. Now we make a new Cast
        // if (inEditMode)
        output = Cast();

        m_CastingDir = Quaternion.Euler(0, m_CurrentAngle, 0) * m_RobotTrans.forward;
        if (Mathf.FloorToInt(m_AngleDeg) == 0)
            Gizmos.color = m_GizmoFrontColor;
        else
            Gizmos.color = m_GizmoColor;

        float drawDistance = output.Distance > 0 ? output.Distance : m_CastingDistance;

        Matrix4x4 cubeTransform = Matrix4x4.TRS(
            m_RobotTrans.position + m_CastingDir * drawDistance + new Vector3(0.0f, m_OffsetHeight, 0.0f),
            m_RobotTrans.rotation * Quaternion.Euler(0, m_CurrentAngle, 0),
            Vector3.one);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
        Gizmos.matrix *= cubeTransform;
        Gizmos.DrawWireCube(Vector3.zero - new Vector3(0f, 0f, 0.01f), new Vector3(m_CastSphereSize, m_CastBoxWidth, m_CastBoxDepth) * 1.5f);
        Gizmos.matrix = oldGizmosMatrix;

        Vector3 startVec = Quaternion.Euler(0, m_CurrentAngle, 0) * m_RobotTrans.forward * drawDistance;
        Gizmos.DrawRay(m_RobotTrans.position + new Vector3(0.0f, m_OffsetHeight, 0.0f), startVec);
    }

    private void DrawObservationRays(PerceptionOutput observations)
    {
        if (rayLine == null)
        {
            // Debug.Log("=== DrawRays at " + String.Format("{0:0.0}: ", m_AngleDeg) + ": rayLine is null ");
            AddLineRenderer();
            rayAdded = true;
        }

        List<Vector3> pos = new List<Vector3>();
        var startPos = m_RobotTrans.localPosition + new Vector3(0.0f, m_OffsetHeight, 0.0f);
        pos.Add(startPos);
        float drawDistance = output.Distance > 0 ? output.Distance : m_CastingDistance;
        Vector3 endPos = m_RobotTrans.localPosition
            + Quaternion.Euler(0, m_CurrentAngle, 0) * m_RobotTrans.forward * drawDistance
            + new Vector3(0.0f, m_OffsetHeight, 0.0f);
        pos.Add(endPos);
        rayLine.startWidth = 0.01f;
        rayLine.endWidth = 0.01f;
        rayLine.SetPositions(pos.ToArray());
        rayLine.useWorldSpace = true;
    }

    private void AddLineRenderer()
    {
        var name = "LineRenderer" + String.Format("{0:0.0}: ", m_AngleDeg);
        rayLine = new GameObject(name).AddComponent<LineRenderer>();
        rayLine.gameObject.transform.SetParent(m_Go.transform, false);
        rayLine.gameObject.layer = 8; // AIBackConGameImageShow layer
    }
}
