using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvaderRayPerception : MonoBehaviour
{

    // private List<ConeCaster> coneCasters = new List<ConeCaster>();
    public ConeCaster coneCaster;

    // void Awake()
    // {
    //     gameObject.AddComponent<ConeCaster>();
    //     coneCaster = gameObject.GetComponent<ConeCaster>();
    // }

    public float[] GetObservations()
    {
        return new float[]{1.0f, 0.0f};
    }

    void Update()
    {
        RaycastHit m_Hit = coneCaster.RobotConeCast();
    }

    // void OnDrawGizmos()
    // {
    //     if(coneCaster == null)
    //     {
    //         gameObject.AddComponent<ConeCaster>();
    //         coneCaster = gameObject.GetComponent<ConeCaster>();
    //     }

    //     RaycastHit m_Hit = coneCaster.RobotConeCast(10.0f, -45.0f, -20.0f);
    // }
}