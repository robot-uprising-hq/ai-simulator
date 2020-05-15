//Put this script on your blue cube.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class PushAgentBasic : Agent
{
    public GameObject area;

    public List<GameObject> arenas = new List<GameObject>();

    [Space(10)]
    public GameObject blockGo;
    public GameObject ballGo;
    /// <summary>
    /// The area bounds.
    /// </summary>
    [HideInInspector]
    public Bounds areaBounds;

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

    /// <summary>
    /// The goal to push the block to.
    /// </summary>
    private GameObject goal;

    /// <summary>
    /// The block to be pushed to the goal.
    /// </summary>
    private GameObject block;

    /// <summary>
    /// Detects when the block touches the goal.
    /// </summary>
    [HideInInspector]
    private GoalDetect goalDetect;

    // public bool useVectorObs;

    /// <summary>
    /// The ground. The bounds are used to spawn the elements.
    /// </summary>
    private GameObject ground;

    PushBlockSettings m_PushBlockSettings;

    Rigidbody m_BlockRb;  //cached on initialization
    Rigidbody m_AgentRb;  //cached on initialization
    Material m_GroundMaterial; //cached on Awake()

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>
    Renderer m_GroundRenderer;

    EnvironmentParameters m_ResetParams;

    private int currentLevel = -1;

    private bool inferenceModeOn = false;
    private float ballTypeDefault = 2.0f;
    private float levelDefault = 3.0f;
    private float rayLengthDefault = 50.0f;
    private float rotationSpeedDefault = 200.0f;
    private float agentSpeedDefault = 20.0f;

    private float m_RotationSpeed;
    private float m_AgentSpeed;

    void Awake()
    {
        m_PushBlockSettings = FindObjectOfType<PushBlockSettings>();

        inferenceModeOn = !Academy.Instance.IsCommunicatorOn;

        if (inferenceModeOn)
        {
            Debug.Log("In Inference mode, setting game settings from PushBlockSettings.");
            ballTypeDefault = m_PushBlockSettings.ballType;
            levelDefault = m_PushBlockSettings.level;
            rayLengthDefault = m_PushBlockSettings.rayLength;
            MaxStep = m_PushBlockSettings.maxSteps;
            rotationSpeedDefault = m_PushBlockSettings.agentRotationSpeed;
            agentSpeedDefault = m_PushBlockSettings.agentRunSpeed;
        }
    }

    public override void Initialize()
    {
        // Cache the agent rigidbody
        m_AgentRb = GetComponent<Rigidbody>();

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(lowerSensor.GetObservations());
        sensor.AddObservation(upperSensor.GetObservations());
    }

    /// <summary>
    /// Use the ground's bounds to pick a random spawn position.
    /// </summary>
    public Vector3 GetRandomSpawnPos()
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            var randomPosX = UnityEngine.Random.Range(-areaBounds.extents.x * m_PushBlockSettings.spawnAreaMarginMultiplier,
                areaBounds.extents.x * m_PushBlockSettings.spawnAreaMarginMultiplier);

            var randomPosZ = UnityEngine.Random.Range(-areaBounds.extents.z * m_PushBlockSettings.spawnAreaMarginMultiplier,
                areaBounds.extents.z * m_PushBlockSettings.spawnAreaMarginMultiplier);
            randomSpawnPos = ground.transform.position + new Vector3(randomPosX, 1f, randomPosZ);
            if (Physics.CheckBox(randomSpawnPos, new Vector3(2.5f, 0.1f, 2.5f)) == false)
            {
                foundNewSpawnLocation = true;
            }
        }
        return randomSpawnPos;
    }

    /// <summary>
    /// Called when the agent moves the block into the goal.
    /// </summary>
    public void ScoredAGoal()
    {
        // We use a reward of 5.
        AddReward(5f);

        // By marking an agent as done AgentReset() will be called automatically.
        EndEpisode();

        // Swap ground material for a bit to indicate we scored.
        StartCoroutine(GoalScoredSwapGroundMaterial(m_PushBlockSettings.goalScoredMaterial, 0.5f));
    }

    /// <summary>
    /// Swap ground material, wait time seconds, then swap back to the regular material.
    /// </summary>
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time);
        m_GroundRenderer.material = m_GroundMaterial;
    }

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
    public void MoveAgent(float[] act)
    {
        if (stopAgent)
        {
            transform.Rotate(Vector3.zero, 0.0f);
            m_AgentRb.velocity = Vector3.zero;
            return;
        }

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = Mathf.FloorToInt(act[0]);
        if (m_ForceAction && m_ActionToForce > -1)
        {
            action = m_ActionToForce;
        }

        // Goalies and Strikers have slightly different action spaces.
        switch (action)
        {
            case 0: // Do nothing
                transform.Rotate(Vector3.zero, 0.0f);
                m_AgentRb.velocity = Vector3.zero;
                break;
            case 1:  // Go forward
                dirToGo = transform.forward * 1f;
                break;
            case 2:  // Go backward
                dirToGo = transform.forward * -1f;
                break;
            case 3:  // Turn right
                rotateDir = transform.up * 1f;
                break;
            case 4:  // Turn left
                rotateDir = transform.up * -1f;
                break;
            // case 5:  // Go forward and turn right
            //     agentSpeed = m_PushBlockSettings.agentMoveAndTurnMoveSpeed;
            //     rotationSpeed = m_PushBlockSettings.agentMoveAndTurnTurnSpeed;
            //     dirToGo = transform.forward * 0.75f;
            //     rotateDir  = transform.up * 1f;
            //     rotationSpeed = 150f;
            //     break;
            // case 6:  // Go forward and turn left
            //     agentSpeed = m_PushBlockSettings.agentMoveAndTurnMoveSpeed;
            //     rotationSpeed = m_PushBlockSettings.agentMoveAndTurnTurnSpeed;
            //     dirToGo = transform.forward * 0.75f;
            //     rotateDir = transform.up * -1f;
            //     rotationSpeed = 150f;
            //     break;
            default:
                Debug.Log("Unknown action: " + action);
                break;
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * m_RotationSpeed);
        m_AgentRb.velocity = dirToGo * m_AgentSpeed;
        // m_AgentRb.AddForce(dirToGo * m_PushBlockSettings.agentRunSpeed,
        //     ForceMode.VelocityChange);
    }

    /// <summary>
    /// Called every step for (var i = 0; i < args.Length - 1; i++)of the engine. Here the agent takes an action.
    /// </summary>
    public override void OnActionReceived(float[] vectorAction)
    {
        // Move the agent using the action.
        MoveAgent(vectorAction);

        // Penalty given each step to encourage agent to finish task quickly.
        AddReward(-1f / MaxStep);
    }

    /// <summary>
    /// Resets the block position and velocities.
    /// </summary>
    void ResetBlock()
    {
        // Get a random position for the block.
        block.transform.position = GetRandomSpawnPos();

        // Reset block velocity back to zero.
        m_BlockRb.velocity = Vector3.zero;

        // Reset block angularVelocity back to zero.
        m_BlockRb.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// In the editor, if "Reset On Done" is checked then AgentReset() will be
    /// called automatically anytime we mark done = true in an agent script.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        SetResetParameters();
        var rotation = UnityEngine.Random.Range(0, 4);
        var rotationAngle = rotation * 90f;
        area.transform.Rotate(new Vector3(0f, rotationAngle, 0f));

        ResetBlock();
        transform.position = GetRandomSpawnPos();
        m_AgentRb.velocity = Vector3.zero;
        m_AgentRb.angularVelocity = Vector3.zero;
    }

    public void SetGroundMaterialFriction()
    {
        var groundCollider = ground.GetComponent<Collider>();

        groundCollider.material.dynamicFriction = m_ResetParams.GetWithDefault("dynamic_friction", 0);
        groundCollider.material.staticFriction = m_ResetParams.GetWithDefault("static_friction", 0);
    }

    public void SetBlockProperties()
    {
        var ballType = (int)m_ResetParams.GetWithDefault("ball_type", ballTypeDefault);
        if(ballType == 1)
        {
            blockGo.SetActive(true);
            ballGo.SetActive(false);
            block = blockGo;
        }
        else
        {
            blockGo.SetActive(false);
            ballGo.SetActive(true);
            block = ballGo;
        }

        goalDetect = block.GetComponent<GoalDetect>();
        goalDetect.agent = this;

        // Cache the block rigidbody
        m_BlockRb = block.GetComponent<Rigidbody>();
    }

    public void SetArena()
    {
        var levelNum = (int)m_ResetParams.GetWithDefault("level", levelDefault);

        if (levelNum == currentLevel) return;

        currentLevel = levelNum;
        for (var i = 0; i < arenas.Count; i++)
        {
            if (i == levelNum)
            {
                arenas[i].SetActive(true);
                // goal = 
                ground = Utils.FindGameObjectInChildWithTag(arenas[i], "ground");
                // Get the ground's bounds
                areaBounds = ground.GetComponent<Collider>().bounds;

                // Get the ground renderer so we can change the material when a goal is scored
                m_GroundRenderer = ground.GetComponent<Renderer>();
                // Starting materialv
                m_GroundMaterial = m_GroundRenderer.material;
            }
            else arenas[i].SetActive(false);
        }
    }

    void SetResetParameters()
    {
        m_RotationSpeed = m_ResetParams.GetWithDefault("agent_rotation_speed", rotationSpeedDefault);
        m_AgentSpeed = m_ResetParams.GetWithDefault("agent_speed", agentSpeedDefault);

        SetArena();
        SetBlockProperties();
        SetGroundMaterialFriction();

        var distance = m_ResetParams.GetWithDefault("ray_length", rayLengthDefault);
        lowerSensor.UpdateCastingDistance(distance);
        upperSensor.UpdateCastingDistance(distance);

        MaxStep = (int)m_ResetParams.GetWithDefault("max_steps", MaxStep);
    }
}
