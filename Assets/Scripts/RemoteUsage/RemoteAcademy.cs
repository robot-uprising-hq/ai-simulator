using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Robotsystemcommunication;


public class RemoteAction
{
    public RemoteAIRobotAgent remoteAgent;
    public int action;
}

[RequireComponent(typeof(GameArena))]
public class RemoteAcademy : MonoBehaviour
{
    [Range(1, 200)]
    [Tooltip("The agent will automatically request a decision every X Academy steps.")]
    public int DecisionPeriod;
    public bool TakeActionsBetweenDecisions = true;

    [Space(10)]
    [Tooltip("If sendScreenCapture is 'false' set the brain server's ip and port")]
    public string brainServerIp;
    public string brainServerPort;

    private int stepCount = 0;
    private UnityBrainServerClient brainServerClient;
    Dictionary<int, RemoteAction> m_RemoteAgents = new Dictionary<int, RemoteAction>();
    BrainActionResponse brainActionRes;

    void Start()
    {
        Academy.Instance.Dispose();
        brainServerClient = new UnityBrainServerClient(brainServerIp, brainServerPort);

        List<GameObject> agents = GetComponent<GameArena>().m_Agents;
        foreach (var agent in agents)
        {
            var script = agent.GetComponent<RemoteAIRobotAgent>();
            var agentID = script.m_ArucoMarkerID;
            var remoteAction = new RemoteAction(){remoteAgent = script, action = 0};
            m_RemoteAgents.Add(agentID, remoteAction);
        }
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
            var actionReq = new BrainActionRequest();
            foreach(KeyValuePair<int, RemoteAction> agent in m_RemoteAgents)
            {
                float[] lowerObs = agent.Value.remoteAgent.GetLowerObservations();
                float[] upperObs = agent.Value.remoteAgent.GetUpperObservations();

                var obs = new Observations(){};
                obs.LowerObservations.AddRange(lowerObs);
                obs.UpperObservations.AddRange(upperObs);
                obs.ArucoMarkerID = agent.Value.remoteAgent.m_ArucoMarkerID;
                actionReq.Observations.Add(obs);
            }

            // Send sensor data to remote brain
            brainActionRes = brainServerClient.GetAction(actionReq);

            MakeActions(brainActionRes);
        }
        else if (TakeActionsBetweenDecisions)
        {
            MakeActions(brainActionRes);
        }

        stepCount++;
    }

    void MakeActions(BrainActionResponse actions)
    {
        foreach(var robotAction in actions.Actions)
        {
            var action = robotAction.Action;
            var arucoMarkerID = robotAction.ArucoMarkerID;
            m_RemoteAgents[arucoMarkerID].remoteAgent.OnActionReceived(new float[] {action});
            
        }   
    }
}
