using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class AIRobotAgent : Agent
{
    public enum Team
    {
        Blue = 0,
        Red = 1
    }

    #region ======= PUBLIC VARIABLES =======
    [Space(10)]
    public RayObservationSensor lowerSensor;
    public RayObservationSensor upperSensor;

    [Space(10)]
    [Header("Debuging and testing options")]
    // For testing purposes you can stop the agent from moving and
    // for example move it manually to see the debug log's sensor values
    public bool stopAgent;
    public bool m_ForceAction;
    public int m_ActionToForce;
    public bool makeSingleAction = false;
    #endregion // ======= END PUBLIC VARIABLES =======


    #region ======= PROTECTED VARIABLES =======
    protected GameArena m_GameArena;
    #endregion // ======= END PROTECTED VARIABLES =======


    #region ======= PRIVATE VARIABLES =======
    // Buffer for actions to be queued to simulate lag in real world where
    // the action sent to robot might be a bit old.
    private ActionLagBuffer m_ActionLagBuffer;

    AIRobotSettings m_AIRobotSettings;
    EnvironmentParameters m_ResetParams;
    Rigidbody m_AgentRb;
    GameObject m_ArenaGO;

    // Rotation
    private float m_RotationSpeed;
    private float m_RotationSpeedRandomFactor;

    // Speed
    private float m_AgentSpeed;
    private float m_AgentSpeedRandomFactor;

    // Move and rotate
    private float m_AgentMoveRotMoveSpeed;
    private float m_AgentMoveRotTurnSpeed;

    #endregion // ======= END PRIVATE VARIABLES =======


    #region ======= UNITY LIFECYCLE FUNCTIONS =======
    void Awake()
    {
        m_ActionLagBuffer = GetComponent<ActionLagBuffer>();
        m_AIRobotSettings = FindObjectOfType<AIRobotSettings>();
        
        m_ArenaGO = transform.parent.gameObject;
        m_GameArena = m_ArenaGO.GetComponent<GameArena>();
    }

    protected virtual void Update()
    {
        // Check for occasional agent getting out of arena
        // and dropping down.
        if (transform.position.y < -0.5f)
        {
            ResetAgent();
        }
    }
    #endregion // ======= END UNITY LIFECYCLE FUNCTIONS =======


    #region ======= PUBLIC FUNCTIONS =======
    public override void Initialize()
    {
        // Cache the agent rigidbody
        m_AgentRb = GetComponent<Rigidbody>();

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        SetAIRobotResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(lowerSensor.GetObservations());
        sensor.AddObservation(upperSensor.GetObservations());
    }

    /// <summary>
    /// In the editor, if "Reset On Done" is checked then AgentReset() will be
    /// called automatically anytime we mark done = true in an agent script.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        m_GameArena.OnEpisodeBegin();
        SetAIRobotResetParameters();
        ResetAgent();
    }

    static float ScaleAction(float act)
    {
        return (Mathf.FloorToInt(act) - 10)/10.0f;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        //m_ActionLagBuffer.InsertAction(vectorAction[0]);
        //float laggedAction = m_ActionLagBuffer.GetAction();

        // This can lead to absolute values of larger than 1, but this is not a problem as long as we ever send -1..1 to the actual robot.
        GetComponent<MovableRobot>().MoveRobot(ScaleAction(vectorAction[0]) * m_AgentSpeed, ScaleAction(vectorAction[1]) * m_AgentSpeed);
        // Penalty given each step to encourage agent to finish task quickly.
        AddReward(-1f / MaxStep);
    }

    ///// <summary>
    ///// Moves the agent according to the selected action.
    ///// </summary>
    //public void MoveAgent(float act)
    //{
    //    // Add a force downwards to compensate the high drag in rigidbody
    //    // for making the gravity very slow
    //    // m_AgentRb.AddForce(transform.up * -1f * 1000f,
    //    //     ForceMode.Force);
    //    if (stopAgent)
    //    {
    //        transform.Rotate(Vector3.zero, 0.0f);
    //        m_AgentRb.velocity = Vector3.zero;
            
    //        if (!makeSingleAction) return;
    //        else
    //        {
    //            actionCounter++;
    //            if (actionCounter > actionCounterMax)
    //            {
    //                makeSingleAction = false;
    //                actionCounter = 0;
    //            }
    //        }
    //    }

    //    var dirToGo = Vector3.zero;
    //    var rotateDir = Vector3.zero;

    //    var action = Mathf.FloorToInt(act);
    //    if (m_ForceAction && m_ActionToForce > -1)
    //    {
    //        action = m_ActionToForce;
    //    }

    //    var agentSpeed = m_AgentSpeed;
    //    var rotationSpeed = m_RotationSpeed;

    //    switch (action)
    //    {
    //        case 0: // Do nothing
    //        //     transform.Rotate(Vector3.zero, 0.0f);
    //        //     m_AgentRb.velocity = Vector3.zero;
    //            break;
    //        case 1:  // Go forward
    //            dirToGo = transform.forward * 1f;
    //            break;
    //        case 2:  // Go backward
    //            dirToGo = transform.forward * -1f;
    //            break;
    //        case 3:  // Turn right
    //            rotateDir = transform.up * 1f;
    //            break;
    //        case 4:  // Turn left
    //            rotateDir = transform.up * -1f;
    //            break;
    //        case 5:  // Go forward and turn right
    //            agentSpeed = m_AgentMoveRotMoveSpeed;
    //            rotationSpeed = m_AgentMoveRotTurnSpeed;
    //            dirToGo = transform.forward * 1f;
    //            rotateDir  = transform.up * 1f;
    //            break;
    //        case 6:  // Go forward and turn left
    //            agentSpeed = m_AgentMoveRotMoveSpeed;
    //            rotationSpeed = m_AgentMoveRotTurnSpeed;
    //            dirToGo = transform.forward * 1f;
    //            rotateDir = transform.up * -1f;
    //            break;
    //        default:
    //            Debug.Log("Unknown action: " + action);
    //            break;
    //    }

    //    // Set agent rotation
    //    transform.Rotate(
    //        rotateDir,
    //        Time.fixedDeltaTime * rotationSpeed * m_RotationSpeedRandomFactor);
    //    // Set agent speed
    //    m_AgentRb.AddForce(dirToGo * agentSpeed * m_AgentSpeedRandomFactor,
    //        ForceMode.VelocityChange);
    //}

    #endregion // ======= END PUBLIC FUNCTIONS =======


    #region ======= PROTECTED FUNCTIONS =======
    protected void ResetAgent()
    {
        // Physics by default updates Transform changes only during Fixed Update which makes Physics.CheckBox
        // to not work correctly when Transform changes and call to Physics.CheckBox are made at the same frame.
        // Physics.SyncTransforms() updates the Transforms to the physics engine and Physics.CheckBox works
        Physics.SyncTransforms();
        var randomRotY = UnityEngine.Random.Range(-180f, 180f);
        transform.rotation = Quaternion.Euler(new Vector3(0, randomRotY, 0));
        transform.position = m_GameArena.GetRandomSpawnPosInArena();
        m_AgentRb.velocity = Vector3.zero;
        m_AgentRb.angularVelocity = Vector3.zero;
    }

    protected void SetAIRobotResetParameters()
    {
        m_RotationSpeed = m_ResetParams.GetWithDefault(
            "agent_rotation_speed",
            m_AIRobotSettings.agentRotationSpeed);
        m_RotationSpeedRandomFactor = m_GameArena.m_AgentSpeedRandomFactor;
        // float rotationSpeedRandom = m_ResetParams.GetWithDefault(
        //     "random_direction",
        //     m_AIRobotSettings.agentRotationSpeedRandom);
        // m_RotationSpeedRandomFactor = Utils.AddRandomFactor(rotationSpeedRandom);

        m_AgentSpeed = m_ResetParams.GetWithDefault(
            "agent_speed",
            m_AIRobotSettings.agentRunSpeed);
        m_AgentSpeedRandomFactor = m_GameArena.m_AgentSpeedRandomFactor;
        // float agentSpeedRandom = m_ResetParams.GetWithDefault(
        //     "random_speed",
        //     m_AIRobotSettings.agentRunSpeedRandom);
        // m_AgentSpeedRandomFactor = Utils.AddRandomFactor(agentSpeedRandom);

        m_AgentMoveRotMoveSpeed = m_ResetParams.GetWithDefault(
            "agent_moverot_move_speed",
            m_AIRobotSettings.agentMoveRotMoveSpeed);
        m_AgentMoveRotTurnSpeed = m_ResetParams.GetWithDefault(
            "agent_moverot_rot_speed",
            m_AIRobotSettings.agentMoveRotTurnSpeed);

        float observationDistanceRandom = m_ResetParams.GetWithDefault(
            "random_obs_dist",
            m_AIRobotSettings.observationDistanceRandom);
        float observationAngleRandom = m_ResetParams.GetWithDefault(
            "random_obs_angle",
            m_AIRobotSettings.observationAngleRandom);

        var distance = m_ResetParams.GetWithDefault(
            "ray_length",
            m_AIRobotSettings.rayLength);
        if (lowerSensor != null)
            lowerSensor.UpdateCasting(distance, observationDistanceRandom, observationAngleRandom);
        if (upperSensor != null)
            upperSensor.UpdateCasting(distance, observationDistanceRandom, observationAngleRandom);

        MaxStep = (int)m_ResetParams.GetWithDefault("max_steps", MaxStep);
    }
    #endregion // ======= END PROTECTED FUNCTIONS =======


    #region ======= PRIVATE FUNCTIONS =======
    
    #endregion // ======= END PRIVATE FUNCTIONS =======
}
