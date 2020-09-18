using UnityEngine;

public class RemoteAIRobotAgent : AIRobotAgent
{
    #region ======= PUBLIC VARIABLES =======
    public bool randomRotateArenaOnReset;

    [HideInInspector]
    public volatile int agentAction = -1;
    public bool m_MakeRayObservations;
    #endregion // ======= END PUBLIC VARIABLES =======


    #region ======= PRIVATE VARIABLES =======
    private int currentStep = 0;
    #endregion // ======= END PRIVATE VARIABLES =======


    #region ======= UNITY LIFECYCLE FUNCTIONS =======
    protected override void Update()
    {
        base.Update();

        if (currentStep > MaxStep)
        {
            OnEpisodeBegin();
            currentStep = 0;
        }
        else
        {
            currentStep++;
        }
        if (agentAction > -1 && !stopAgent)
        {
            MoveAgent(agentAction);
        }
        if (m_MakeRayObservations)
        {
            GetLowerObservations();
            GetUpperObservations();
        }
    }
    #endregion // ======= END UNITY LIFECYCLE FUNCTIONS =======


    #region ======= PUBLIC FUNCTIONS =======
    /// <summary>
    /// In the editor, if "Reset On Done" is checked then AgentReset() will be
    /// called automatically anytime we mark done = true in an agent script.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        m_GameArena.OnEpisodeBegin(true, randomRotateArenaOnReset);
        SetAIRobotResetParameters();
        ResetAgent();

    }
    public float[] GetLowerObservations()
    {
        return lowerSensor.GetObservations();
    }

    public float[] GetUpperObservations()
    {
        return upperSensor.GetObservations();
    }
    #endregion // ======= END PUBLIC FUNCTIONS =======
}