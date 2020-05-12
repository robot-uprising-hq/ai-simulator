using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class RemoteAcademy : MonoBehaviour
{
    [Range(1, 200)]
    [Tooltip("The agent will automatically request a decision every X Academy steps.")]
    public int DecisionPeriod = 50;
    public bool TakeActionsBetweenDecisions = true;

    public RemoteAgent remoteAgent;

    [Space(10)]
    [Tooltip("If sendScreenCapture is 'false' set the brain server's ip and port")]
    public string brainServerIp;
    public string brainServerPort;

    private int stepCount = 0;
    private UnityBrainServerClient brainServerClient;
    private int action = 0;

    void Awake()
    {
        Academy.Instance.Dispose();
        brainServerClient = new UnityBrainServerClient(brainServerIp, brainServerPort);
    }

    // Update is called once per frame
    void Update()
    {
        EnvironmentStep();
    }

    void EnvironmentStep()
    {
        if (stepCount % DecisionPeriod == 0)
        {
            float[] lowerObs = remoteAgent.GetLowerObservations();
            float[] upperObs = remoteAgent.GetUpperObservations();
            // Send sensor data to remote brain
            action = brainServerClient.GetAction(lowerObs, upperObs);
            remoteAgent.OnActionReceived(new float[] {action});
        }

        if (TakeActionsBetweenDecisions)
        {
            remoteAgent.OnActionReceived(new float[] {action});
        }

        stepCount++;
    }
}
