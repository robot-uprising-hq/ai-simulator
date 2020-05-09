using UnityEngine;
using System.Collections;

public class ConeCaster : MonoBehaviour
{
    public Color m_GizmoColor = Color.red;
    public float m_MaxDistance;
    public float m_StartAngleDeg;
    public float m_EndAngleDeg;

    float m_CastingAngleWidthDeg;
    float m_CastingDirAngleDeg;
    RaycastHit m_Hit;
    Vector3 m_CastingDir;
    float castHeight = 0.5f;
    float castDepth = 0.1f;
    bool m_HitDetect;
    float m_CastWidth;
 
    public RaycastHit RobotConeCast()
    {
        m_CastingAngleWidthDeg = Mathf.Abs(m_StartAngleDeg - m_EndAngleDeg);
        m_CastWidth = (Mathf.Tan(m_CastingAngleWidthDeg * Mathf.Deg2Rad / 2) * m_MaxDistance) * 2;
        m_CastingDirAngleDeg = m_EndAngleDeg - m_CastingAngleWidthDeg / 2;
        m_CastingDir = Quaternion.Euler(0, m_CastingDirAngleDeg, 0) * transform.forward;

        m_HitDetect = Physics.BoxCast(
            transform.position,
            new Vector3(m_CastWidth/2, 0.5f, 0.1f),
            m_CastingDir,
            out m_Hit,
            transform.rotation * Quaternion.Euler(0, m_CastingDirAngleDeg, 0),
            m_MaxDistance);
        if (m_HitDetect)
        {
            float hitDistance = Vector3.Distance(transform.position, m_Hit.point);
            Vector3 directionToHit = m_Hit.point - transform.position;
            float angleToHit = Vector3.Angle(transform.forward, directionToHit);
            string name = "";

            if (hitDistance > m_MaxDistance)
            {
                 name = "Nada";
            }
            else if (HitBetweenAngles(m_Hit, m_StartAngleDeg, m_EndAngleDeg))
            {
                name = m_Hit.collider.name;
            }
            else if (CornerCase(m_Hit, m_StartAngleDeg, m_EndAngleDeg))
            {
                name = m_Hit.collider.name + ", Edgecase";
            }
            Debug.LogFormat(
                "Hit {0} in: {1:0.0}/{2:0.0}, angleToHit: {3:0.0}, hitDistance: {4:0.0}",
                name,
                m_StartAngleDeg,
                m_EndAngleDeg,
                angleToHit,
                hitDistance);
        }
        return m_Hit;
    }

    bool HitBetweenAngles(RaycastHit hit, float startAngle, float endAngle)
    {
        Vector3 directionToHit = hit.point - transform.position;
        Vector3 startVec = Quaternion.Euler(0, startAngle, 0) * transform.forward;
        Vector3 endVec = Quaternion.Euler(0, endAngle, 0) * transform.forward;

        float angleToHit = Vector3.Angle(transform.forward, directionToHit);
        float lowerAngle = Vector3.Angle(transform.forward, startVec);
        float upperAngle = Vector3.Angle(transform.forward, endVec);

        return angleToHit < lowerAngle && angleToHit > upperAngle;
    }

    bool CornerCase(RaycastHit hit, float startAngle, float endAngle)
    {
        Vector3 startVec = Quaternion.Euler(0, startAngle, 0) * transform.forward;
        Vector3 endVec = Quaternion.Euler(0, endAngle, 0) * transform.forward;
        Ray startRay = new Ray(transform.position, startVec);
        bool startHit = hit.collider.bounds.IntersectRay(startRay);
        Ray endRay = new Ray(transform.position, endVec);
        bool endHit = hit.collider.bounds.IntersectRay(endRay);
        return startHit || endHit;
    }

    void OnDrawGizmos()
    {
        m_CastingAngleWidthDeg = Mathf.Abs(m_StartAngleDeg - m_EndAngleDeg);
        m_CastWidth = Mathf.Tan(m_CastingAngleWidthDeg * Mathf.Deg2Rad / 2) * m_MaxDistance * 2;
        m_CastingDirAngleDeg = m_EndAngleDeg - m_CastingAngleWidthDeg / 2;
        m_CastingDir = Quaternion.Euler(0, m_CastingDirAngleDeg, 0) * transform.forward;

        Gizmos.color = m_GizmoColor;

        Matrix4x4 cubeTransform = Matrix4x4.TRS(
            transform.position + m_CastingDir * m_MaxDistance,
            transform.rotation * Quaternion.Euler(0, m_CastingDirAngleDeg, 0),
            transform.lossyScale);
        Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
        Gizmos.matrix *= cubeTransform;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(m_CastWidth, 0.5f, 0.1f));

        Gizmos.matrix = oldGizmosMatrix;
        float sideLength = m_MaxDistance / Mathf.Cos(m_CastingAngleWidthDeg / 2 * Mathf.Deg2Rad);
        Vector3 startVec = Quaternion.Euler(0, m_StartAngleDeg, 0) * transform.forward * sideLength;
        Gizmos.DrawRay(transform.position, startVec);
        Vector3 endVec = Quaternion.Euler(0, m_EndAngleDeg, 0) * transform.forward * sideLength;
        Gizmos.DrawRay(transform.position, endVec);
        Vector3 castVec = m_CastingDir * m_MaxDistance;
        Gizmos.DrawRay(transform.position, castVec);
    }
}
