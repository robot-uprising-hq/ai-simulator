using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PerceptionOutput
{
    public float Distance;
    public GameObject hitGo;
}

public class SectorCaster
{
    private Color m_GizmoColor = Color.red;
    Transform m_RobotTrans;
    float m_OffsetHeight;
    float m_MaxDistance;
    float m_StartAngleDeg;
    float m_EndAngleDeg;
    List<string> m_DetectableTags;

    float m_CastingAngleWidthDeg;
    float m_CastingDirAngleDeg;
    RaycastHit m_Hit;
    Vector3 m_CastingDir;
    float castHeight = 0.5f;
    float castDepth = 0.1f;
    bool m_HitDetect;
    float m_CastWidth;

    public SectorCaster(Transform robotTrans, float maxDistance, float offsetHeight, float startAngle, float endAngle, List<string> detectableTags)
    {
        m_RobotTrans = robotTrans;
        m_OffsetHeight = offsetHeight;
        m_StartAngleDeg = startAngle;
        m_EndAngleDeg = endAngle;
        m_DetectableTags = detectableTags;
        m_CastingAngleWidthDeg = Mathf.Abs(m_StartAngleDeg - m_EndAngleDeg);
        m_MaxDistance = Mathf.Cos(m_CastingAngleWidthDeg * Mathf.Deg2Rad / 2) * maxDistance;
        m_CastWidth = (Mathf.Tan(m_CastingAngleWidthDeg * Mathf.Deg2Rad / 2) * m_MaxDistance) * 2;
        m_CastingDirAngleDeg = m_EndAngleDeg - m_CastingAngleWidthDeg / 2;
        // m_CastingDir = Quaternion.Euler(0, m_CastingDirAngleDeg, 0) * m_RobotTrans.forward;
    }

    public float[] GetObservations()
    {
        float[] observations = new float[m_DetectableTags.Count + 2];
        PerceptionOutput output = Cast();
        
        if (output.hitGo != null)
        {
            // Find the index of the tag of the object that was hit.
            for (var i = 0; i < m_DetectableTags.Count; i++)
            {
                if (output.hitGo.CompareTag(m_DetectableTags[i]))
                {
                    observations[i] = 1.0f;
                    observations[observations.Length - 1] = output.Distance / m_MaxDistance;
                    break;
                }
            }
        }
        else
        {
            observations[observations.Length - 2] = 1.0f;
        }

        Debug.LogFormat(
            "m_StartAngleDeg: {0:0.0}, observations: {1}",
            m_StartAngleDeg,
            string.Join(":", observations));
        return observations;
    }
 
    public PerceptionOutput Cast()
    {
        m_CastingDir = Quaternion.Euler(0, m_CastingDirAngleDeg, 0) * m_RobotTrans.forward;
        m_HitDetect = Physics.BoxCast(
            m_RobotTrans.position + new Vector3(0.0f, m_OffsetHeight, 0.0f),
            new Vector3(m_CastWidth / 2, castHeight, castDepth),
            m_CastingDir,
            out m_Hit,
            m_RobotTrans.rotation * Quaternion.Euler(0, m_CastingDirAngleDeg, 0),
            m_MaxDistance);

        PerceptionOutput output = new PerceptionOutput();
        if (m_HitDetect)
        {
            float hitDistance = Vector3.Distance(m_RobotTrans.position, m_Hit.point);
            if (hitDistance > m_MaxDistance)
            {
                output.Distance = 0.0f;
                output.hitGo = null;
            }
            else if (HitBetweenAngles(m_Hit, m_StartAngleDeg, m_EndAngleDeg))
            {
                output.Distance = hitDistance;
                output.hitGo = m_Hit.collider.gameObject;
            }
            else if (CornerCase(m_Hit, m_StartAngleDeg, m_EndAngleDeg))
            {
                output.Distance = hitDistance;
                output.hitGo = m_Hit.collider.gameObject;
            }
        }
        else
        {
            output.Distance = 0.0f;
            output.hitGo = null;
        }

        return output;
    }

    bool HitBetweenAngles(RaycastHit hit, float startAngle, float endAngle)
    {
        Vector3 directionToHit = hit.point - m_RobotTrans.position;
        Vector3 startVec = Quaternion.Euler(0, startAngle, 0) * m_RobotTrans.forward;
        Vector3 endVec = Quaternion.Euler(0, endAngle, 0) * m_RobotTrans.forward;

        float angleToHit = Vector3.Angle(m_RobotTrans.forward, directionToHit);
        float side = AngleDir(m_RobotTrans.forward, directionToHit, m_RobotTrans.up);
        // float lowerAngle = Vector3.Angle(m_RobotTrans.forward, startVec);
        // float upperAngle = Vector3.Angle(m_RobotTrans.forward, endVec);

        Debug.LogFormat(
            "Sector {0:0}/{1:0}: angleToHit: {2:0.0}",
            m_StartAngleDeg,
            m_EndAngleDeg,
            angleToHit * side);
        return angleToHit * side < m_EndAngleDeg && angleToHit * side > m_StartAngleDeg;
    }

    //returns -1 when to the left, 1 to the right
    public float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);
 
        if (dir >= 0.0f) {
            return 1.0f;
        } else
        { //if (dir < 0.0f) {
            return -1.0f;
        }
    }

    bool CornerCase(RaycastHit hit, float startAngle, float endAngle)
    {
        Vector3 startVec = Quaternion.Euler(0, startAngle, 0) * m_RobotTrans.forward;
        Vector3 endVec = Quaternion.Euler(0, endAngle, 0) * m_RobotTrans.forward;
        Ray startRay = new Ray(m_RobotTrans.position, startVec);
        bool startHit = hit.collider.bounds.IntersectRay(startRay);
        Ray endRay = new Ray(m_RobotTrans.position, endVec);
        bool endHit = hit.collider.bounds.IntersectRay(endRay);
        return startHit || endHit;
    }

    public void DrawCasterGizmos()
    {
        m_CastingDir = Quaternion.Euler(0, m_CastingDirAngleDeg, 0) * m_RobotTrans.forward;
        Gizmos.color = m_GizmoColor;

        Matrix4x4 cubeTransform = Matrix4x4.TRS(
            m_RobotTrans.position + m_CastingDir * m_MaxDistance,
            m_RobotTrans.rotation * Quaternion.Euler(0, m_CastingDirAngleDeg, 0),
            m_RobotTrans.lossyScale);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
        Gizmos.matrix *= cubeTransform;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(m_CastWidth, castHeight, castDepth));

        Gizmos.matrix = oldGizmosMatrix;
        float sideLength = m_MaxDistance / Mathf.Cos(m_CastingAngleWidthDeg / 2 * Mathf.Deg2Rad);
        Vector3 startVec = Quaternion.Euler(0, m_StartAngleDeg, 0) * m_RobotTrans.forward * sideLength;
        Gizmos.DrawRay(m_RobotTrans.position, startVec);
        Vector3 endVec = Quaternion.Euler(0, m_EndAngleDeg, 0) * m_RobotTrans.forward * sideLength;
        Gizmos.DrawRay(m_RobotTrans.position, endVec);

        // Gizmos.color = Color.green;
        // Vector3 castVec = m_CastingDir * m_MaxDistance;
        // Gizmos.DrawRay(m_RobotTrans.position, castVec);
    }
}
