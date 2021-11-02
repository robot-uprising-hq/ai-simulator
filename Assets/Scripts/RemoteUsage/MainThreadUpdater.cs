using UnityEngine;

public class MainThreadUpdater: MonoBehaviour
{
    RemoteAIRobotAgent agent;
    bool m_NewTransformAvailable = false;
    bool m_MakeObservations = false;
    Vector3 newPos;
    Quaternion newRot;
    bool activeState = true;
    Vector3 currentPosition;
    Quaternion currentRotation;
    float[] lowerObservations;
    float[] upperObservations;

    void Awake()
    {
        agent = GetComponent<RemoteAIRobotAgent>();
    }

    void Update()
    {   
        currentPosition = gameObject.transform.localPosition;
        currentRotation  = gameObject.transform.localRotation;

        if (m_NewTransformAvailable == true)
        {
            m_NewTransformAvailable = false;
            gameObject.transform.localPosition = newPos;
            gameObject.transform.localRotation = newRot;
        }

        if (m_MakeObservations == true)
        {
            m_MakeObservations = false;
            lowerObservations= agent.GetLowerObservations();
            upperObservations = agent.GetUpperObservations();
        }
        // else
        // {

        // }
    }

    public void SetTransformToBeUpdated(Vector3 newPos, Quaternion newRot)
    {
        this.newPos = newPos;
        this.newRot = newRot;
        m_NewTransformAvailable = true;
    }

    public void SetActiveState(bool activeState)
    {
        this.activeState = activeState;
    }

    public bool GetActiveState()
    {
        return activeState;
    }

    public Vector3 GetPosition()
    {
        return currentPosition;
    }

    public Vector3 GetRotation()
    {
        return currentRotation.eulerAngles;
    }

    public float[] GetLowerObservations()
    {
        // return new float[]{0.0f, 1.0f};
        return lowerObservations;
    }

    public float[] GetUpperObservations()
    {
        // return new float[]{0.0f, 1.0f};
        return upperObservations;
    }

    public void MakeObservations()
    {
        m_MakeObservations = true;
    }
}
