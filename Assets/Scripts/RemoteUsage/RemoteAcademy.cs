using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class RemoteAcademy : MonoBehaviour
{
    [Range(1, 200)]
    [Tooltip("The agent will automatically request a decision every X Academy steps.")]
    public int DecisionPeriod = 50;

    public RemoteAgent remoteAgent;
    public ScreenStreamer screenStreamer;

    [Space(10)]
    [Tooltip("If sendScreenCapture is 'false' set the brain server's ip and port")]
    public string brainServerIp;
    public string brainServerPort;

    private int stepCount = 0;
    private UnityBrainServerClient brainServerClient;

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
        stepCount = stepCount + 1;
        if (stepCount < DecisionPeriod) return;

        stepCount = 0;

        int action = 0;

        float[] lowerObs = remoteAgent.GetLowerObservations();
        float[] upperObs = remoteAgent.GetUpperObservations();
        // Send sensor data to remote brain
        action = brainServerClient.GetAction(lowerObs, upperObs);

        remoteAgent.AgentAction(new float[] {action});
    }
}
