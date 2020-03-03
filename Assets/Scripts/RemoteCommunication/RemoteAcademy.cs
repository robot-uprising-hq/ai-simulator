using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Braincommunication;

public class RemoteAcademy : MonoBehaviour
{
    [Range(1, 200)]
    [Tooltip("The agent will automatically request a decision every X Academy steps.")]
    public int DecisionPeriod = 50;

    public RemoteAgent remoteAgent;

    private int stepCount = 0;
    private BrainClient brainClient;
    // Start is called before the first frame update

    void Start()
    {
        brainClient = new BrainClient();
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
        float[] lowerObs = remoteAgent.GetLowerObservations();
        float[] upperObs = remoteAgent.GetUpperObservations();

        // Send sensor data to remote brain
        int action = brainClient.GetAction(lowerObs, upperObs);

        remoteAgent.AgentAction(new float[] {action});
    }
}
