//Put this script on your blue cube.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class RemoteAgent : MonoBehaviour
{
    public bool randomRotateArenaOnReset;

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
    public bool m_MakeRayObservations;

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
    private int currentStep = 0;
    private float ballTypeDefault = 2.0f;
    private float levelDefault = 3.0f;
    private float rayLengthDefault = 50.0f;
    private float m_SpawnAreaMarginMultiplier;
    private int MaxStep = 5000;

    // Rotation
    private float m_RotationSpeed;
    private float m_RotationSpeedRandom;

    // Speed
    private float m_AgentSpeed;
    private float m_AgentSpeedRandom;

    // Move and rotate
    private float m_AgentMoveRotMoveSpeed;
    private float m_AgentMoveRotTurnSpeed;

    [HideInInspector]
    public volatile int agentAction = -1;

    void Awake()
    {
        m_PushBlockSettings = FindObjectOfType<PushBlockSettings>();

        Debug.Log("In Inference mode, setting game settings from PushBlockSettings.");

        Initialize();
    }

    void Update()
    {
        // MoveAgent(action);
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
            MoveAgent(new float[]{agentAction});
        
        if (m_MakeRayObservations)
        {
            GetLowerObservations();
            GetUpperObservations();
        }
    }

    public float[] GetLowerObservations()
    {
        return lowerSensor.GetObservations();
    }

    public float[] GetUpperObservations()
    {
        return upperSensor.GetObservations();
    }

    public void Initialize()
    {
        // Cache the agent rigidbody
        m_AgentRb = GetComponent<Rigidbody>();

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        SetResetParameters();
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
            var randomPosX = UnityEngine.Random.Range(
                -areaBounds.extents.x * m_SpawnAreaMarginMultiplier,
                areaBounds.extents.x * m_SpawnAreaMarginMultiplier);

            var randomPosZ = UnityEngine.Random.Range(
                -areaBounds.extents.z * m_SpawnAreaMarginMultiplier,
                areaBounds.extents.z * m_SpawnAreaMarginMultiplier);
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
        // AddReward(5f);
        
        // By marking an agent as done AgentReset() will be called automatically.
        OnEpisodeBegin();

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

        var agentSpeed = m_AgentSpeed;
        var rotationSpeed = m_RotationSpeed;
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
            case 5:  // Go forward and turn right
                agentSpeed = m_AgentMoveRotMoveSpeed;
                rotationSpeed = m_AgentMoveRotTurnSpeed;
                dirToGo = transform.forward * 1f;
                rotateDir  = transform.up * 1f;
                break;
            case 6:  // Go forward and turn left
                agentSpeed = m_AgentMoveRotMoveSpeed;
                rotationSpeed = m_AgentMoveRotTurnSpeed;
                dirToGo = transform.forward * 1f;
                rotateDir = transform.up * -1f;
                break;
            default:
                Debug.Log("Unknown action: " + action);
                break;
        }

        transform.Rotate(
            rotateDir,
            Time.fixedDeltaTime * (rotationSpeed + AddRandomFactor(rotationSpeed, m_RotationSpeedRandom)));
        m_AgentRb.velocity = dirToGo * (agentSpeed + AddRandomFactor(agentSpeed, m_AgentSpeedRandom));
    }

    /// <summary>
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
    public void OnActionReceived(float[] vectorAction)
    {
        MoveAgent(vectorAction);
    }

    float AddRandomFactor(float baseValue, float randomFactor)
    {
        float randomMultiplier = UnityEngine.Random.Range(-randomFactor, randomFactor);
        return baseValue * randomMultiplier;
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
    public void OnEpisodeBegin()
    {
        SetResetParameters();

        if (randomRotateArenaOnReset)
        {
            var rotation = UnityEngine.Random.Range(0, 4);
            var rotationAngle = rotation * 90f;
            area.transform.Rotate(new Vector3(0f, rotationAngle, 0f));
        }

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
        goalDetect.remoteAgent = this;

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
        ballTypeDefault = m_PushBlockSettings.ballType;
        levelDefault = m_PushBlockSettings.level;
        rayLengthDefault = m_PushBlockSettings.rayLength;
        MaxStep = m_PushBlockSettings.maxSteps;

        m_RotationSpeed = m_PushBlockSettings.agentRotationSpeed;
        m_RotationSpeedRandom = m_PushBlockSettings.agentRotationSpeedRandom;
        m_AgentSpeed = m_PushBlockSettings.agentRunSpeed;
        m_AgentSpeedRandom = m_PushBlockSettings.agentRunSpeedRandom;
        m_AgentMoveRotMoveSpeed = m_PushBlockSettings.agentMoveRotMoveSpeed;
        m_AgentMoveRotTurnSpeed = m_PushBlockSettings.agentMoveRotTurnSpeed;
        m_SpawnAreaMarginMultiplier = m_PushBlockSettings.spawnAreaMarginMultiplier;

        SetArena();
        SetBlockProperties();
        SetGroundMaterialFriction();

        var distance = m_ResetParams.GetWithDefault("ray_length", rayLengthDefault);
        if (lowerSensor != null) lowerSensor.UpdateCasting(distance, 0.0f, 0.0f);
        if (upperSensor != null) upperSensor.UpdateCasting(distance, 0.0f, 0.0f);

        MaxStep = (int)m_ResetParams.GetWithDefault("max_steps", MaxStep);
    }
}
