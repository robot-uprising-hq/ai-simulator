using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

[System.Serializable]
public class EnergyCore
{
    public GameObject coreGO;
    public Rigidbody coreRb;
    public EnergyCoreController.CoreShape coreShape;
}

public class AIRobot
{
    public GameObject robotGO;
    [HideInInspector]
    public AIRobotAgent robotScript;
    public int arucoMarkerID;
}


class Rewards{
    public int blueReward;
    public int redReward;
}

public class GameArena : MonoBehaviour
{
    #region ======= PUBLIC VARIABLES =======
    public List<GameObject> m_Arenas = new List<GameObject>();

    [Space(10)]
    public bool m_AssignRobotsManually;
    public List<GameObject> m_Agents = new List<GameObject>();

    [Space(10)]
    public bool m_RotateArenaOnEpisodeBegin;

    [Space(10)]
    public GameObject m_BlueRobotPrefab;
    public GameObject m_RedRobotPrefab;

    [Space(10)]
    public GameObject m_PositiveEnergyCoreBlockPrefab;
    public GameObject m_PositiveEnergyCoreBallPrefab;
    public GameObject m_NegativeEnergyCoreBlockPrefab;
    public GameObject m_NegativeEnergyCoreBallPrefab;

    [Space(10)]
    [Header("Training options")]
    public bool m_EndEpisodeOnNegRewardIfSingleAgent;

    [HideInInspector]
    public float m_AgentSpeedRandomFactor = 1.0f;
    public float m_RotationSpeedRandomFactor = 1.0f;
    #endregion // ======= END PUBLIC VARIABLES =======


    #region ======= PRIVATE VARIABLES =======
    List<AIRobot> m_BlueAgents = new List<AIRobot>();
    List<AIRobot> m_RedAgents = new List<AIRobot>();
    List<EnergyCore> m_PosEnergyCores = new List<EnergyCore>();
    List<EnergyCore> m_NegEnergyCores = new List<EnergyCore>();
    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>
    Renderer m_GroundRenderer;
    Material m_GroundMaterial;
    GameObject m_Ground;
    EnvironmentParameters m_ResetParams;
    AIRobotSettings m_AIRobotSettings;
    Bounds m_AreaBounds;
    int currentLevel = -1;
    int resetCounter = 0;
    int goalCounter = 0;

    #endregion // ======= END PRIVATE VARIABLES =======


    #region ======= UNITY LIFECYCLE FUNCTIONS =======
    void Awake()
    {
        if (m_AssignRobotsManually)
        {
            foreach (var agent in m_Agents)
            {
                var aiRobot = new AIRobot(){robotGO = agent, robotScript = agent.GetComponent<AIRobotAgent>()};
                if (agent.gameObject.CompareTag("red_agent")) m_RedAgents.Add(aiRobot);
                else if (agent.gameObject.CompareTag("blue_agent")) m_BlueAgents.Add(aiRobot);
            }
        }
    }
    void Start()
    {
        m_AIRobotSettings = FindObjectOfType<AIRobotSettings>();
        m_ResetParams = Academy.Instance.EnvironmentParameters;

        OnEpisodeBegin();
    }
    #endregion // ======= END UNITY LIFECYCLE FUNCTIONS =======


    #region ======= PUBLIC FUNCTIONS =======
    public void OnEpisodeBegin()
    {
        resetCounter++;
        // Only reset environment once in the first call of this function. Not on every agent's call to this function.
        // If called with forceInit, ignore this and initialize the game.
        if (resetCounter == 1)
        {
            if (m_RotateArenaOnEpisodeBegin)
            {
                var rotation = UnityEngine.Random.Range(0, 4);
                var rotationAngle = rotation * 90f;
                transform.Rotate(new Vector3(0f, rotationAngle, 0f));
            }
            float agentSpeedRandom = m_ResetParams.GetWithDefault(
                "random_speed",
                m_AIRobotSettings.agentRunSpeedRandom);
            m_AgentSpeedRandomFactor = Utils.AddRandomFactor(agentSpeedRandom);

            float rotationSpeedRandom = m_ResetParams.GetWithDefault(
                "random_direction",
                m_AIRobotSettings.agentRotationSpeedRandom);
            m_RotationSpeedRandomFactor = Utils.AddRandomFactor(rotationSpeedRandom);

            SetArena();
            if (!m_AssignRobotsManually) SetAgents();
            SetEnergyCores();
        }

        // Check if all agents have called this function
        if (resetCounter == m_BlueAgents.Count + m_RedAgents.Count)
        {
            resetCounter = 0;
        }
    }

    /// <summary>
    /// Use the ground's bounds to pick a random spawn position.
    /// </summary>
    public Vector3 GetRandomSpawnPosInArena()
    {
        var m_SpawnAreaMarginMultiplier = m_ResetParams.GetWithDefault(
            "spawn_area_margin",
            m_AIRobotSettings.spawnAreaMarginMultiplier);
        var maxTries = 100;
        var tryCount = 0;
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        while (foundNewSpawnLocation == false)
        {
            var randomPosX = UnityEngine.Random.Range(
                -m_AreaBounds.extents.x * m_SpawnAreaMarginMultiplier,
                m_AreaBounds.extents.x * m_SpawnAreaMarginMultiplier);

            var randomPosZ = UnityEngine.Random.Range(
                -m_AreaBounds.extents.z * m_SpawnAreaMarginMultiplier,
                m_AreaBounds.extents.z * m_SpawnAreaMarginMultiplier);
            randomSpawnPos = m_Ground.transform.position + new Vector3(randomPosX, 0.1f, randomPosZ);
            if (Physics.CheckBox(randomSpawnPos, new Vector3(0.075f, 0.065f, 0.075f)) == false)
            {
                foundNewSpawnLocation = true;
            }
            tryCount++;
            if (tryCount > maxTries) throw new Exception("Could not find new spawn position");
        }
        return randomSpawnPos;
    }
    
    /// <summary>
    /// Gives rewards to agents based on which kind of energycore hit which goal.
    /// goalHit: Which goal was hit
    /// coreType: Which type of energy core hit the goal
    /// </summary>
    public void GoalTouched(AIRobotAgent.Team goalHit, EnergyCoreController.CoreType coreType)
    {
        goalCounter++;
        var rewards = CalculateRewards(goalHit, coreType);
        
        foreach (var agent in m_BlueAgents)
        {
            agent.robotScript.AddReward(rewards.blueReward);
        }
        foreach (var agent in m_RedAgents)
        {
            agent.robotScript.AddReward(rewards.redReward);
        }

        var amountOfAgents = m_BlueAgents.Count + m_RedAgents.Count;
        if (amountOfAgents == 1 && m_EndEpisodeOnNegRewardIfSingleAgent)
        {
            if (m_BlueAgents.Count > 0 && rewards.blueReward < 0) EndEpisodeForAgents();
            else if (m_RedAgents.Count > 0 && rewards.redReward < 0) EndEpisodeForAgents();
        }
        // All energy cores have been put in goals. Reset arena.
        if (goalCounter >= (m_NegEnergyCores.Count + m_PosEnergyCores.Count))
        {
            EndEpisodeForAgents();
        }

        Material groundMat = null;
        if (rewards.blueReward > 0) groundMat = m_AIRobotSettings.blueTeamSuccessMat;
        else groundMat = m_AIRobotSettings.redTeamSuccessMat;
        StartCoroutine(GoalScoredSwapGroundMaterial(groundMat, 0.5f));
    }
    #endregion // ======= END PUBLIC FUNCTIONS =======


    #region ======= PRIVATE FUNCTIONS =======
    void EndEpisodeForAgents()
    {
        foreach (var agent in m_BlueAgents)
        {
            agent.robotScript.EndEpisode();
        }
        foreach (var agent in m_RedAgents)
        {
            agent.robotScript.EndEpisode();
        }
        goalCounter = 0;
    }
    Rewards CalculateRewards(AIRobotAgent.Team goalHit, EnergyCoreController.CoreType coreType)
    {
        int redReward = 0;
        int blueReward = 0;
        
        if (goalHit == AIRobotAgent.Team.Blue)
        {
            // Blue team got positive core.
            if (coreType == EnergyCoreController.CoreType.Positive)
            {
                redReward = m_AIRobotSettings.posECoreHitsOpponentGoalReward;
                blueReward = m_AIRobotSettings.posECoreHitsOwnGoalReward;
            }
            // Blue team got negative core.
            else
            {
                redReward = m_AIRobotSettings.negECoreHitsOpponentGoalReward;
                blueReward = m_AIRobotSettings.negECoreHitsOwnGoalReward;
            }
        }
        else
        {
            // Red team got positive core.
            if (coreType == EnergyCoreController.CoreType.Positive)
            {
                redReward = m_AIRobotSettings.posECoreHitsOwnGoalReward;
                blueReward = m_AIRobotSettings.posECoreHitsOpponentGoalReward;
            }
            // Red team got negative core.
            else
            {
                redReward = m_AIRobotSettings.negECoreHitsOwnGoalReward;
                blueReward = m_AIRobotSettings.negECoreHitsOpponentGoalReward;
            }
        }

        var rewards = new Rewards(){blueReward = blueReward, redReward = redReward};
        return rewards;
    }
    void SetAgents()
    {
        var nRedAgents = (int)m_ResetParams.GetWithDefault(
            "number_of_red_agents",
            m_AIRobotSettings.numberOfRedAgents);
        var nBlueAgents = (int)m_ResetParams.GetWithDefault(
            "number_of_blue_agents",
            m_AIRobotSettings.numberOfBlueAgents);

        int amountRedChanged = nRedAgents - m_RedAgents.Count;
        int amountBlueChanged = nBlueAgents - m_BlueAgents.Count;
        if (amountRedChanged != 0)
        {
            InitializeAgents(m_RedRobotPrefab, m_RedAgents, amountRedChanged);
        }
        if (amountBlueChanged != 0)
        {
            InitializeAgents(m_BlueRobotPrefab, m_BlueAgents, amountBlueChanged);
        }
    }

    void InitializeAgents(GameObject agentPrefab, List<AIRobot> list, int amountChanged)
    {
        if (amountChanged < 0)
        {   
            Debug.Log("Amount of new agents: " + amountChanged);
            throw new Exception("Cannot remove Agents. Not implemented yet");
        }
        for (int i = 0; i < amountChanged; i++)
        {
            var randomRotY = UnityEngine.Random.Range(-180f, 180f);
            var randomRotQuat = Quaternion.Euler(new Vector3(0, randomRotY, 0));
            var agentGO = GameObject.Instantiate(agentPrefab, GetRandomSpawnPosInArena(), randomRotQuat, transform);
            var agent = new AIRobot(){robotGO = agentGO, robotScript = agentGO.GetComponent<AIRobotAgent>()};
            list.Add(agent);
        }
    }
    /// <summary>
    /// Initialize the energy cores in the given list if any change to the amout of cores
    /// or shape of cores have occured.
    /// Also Give energy cores new random position and random rotation.
    /// </summary>
    void InitializeEnergyCores(GameObject corePrefab, List<EnergyCore> list, int nbrOfCores, EnergyCoreController.CoreShape shape)
    {
        bool amountChanged = list.Count != nbrOfCores;
        bool shapeChanged =  list.Count == 0 || list[0].coreShape != shape;

        if (amountChanged || shapeChanged)
        {
            foreach (var core in list)
            {
                core.coreRb = null;
                Destroy(core.coreGO);
            }

            list.Clear(); 
            for (int i = 0; i < nbrOfCores; i++)
            {
                var go = GameObject.Instantiate(corePrefab, transform);
                var core = new EnergyCore();
                core.coreGO = go;
                core.coreRb = go.GetComponent<Rigidbody>();
                list.Add(core);
            }
        }

        foreach (var core in list)
        {
            // Physics by default updates Transform changes only during Fixed Update which makes Physics.CheckBox
            // to not work correctly when Transform changes and call to Physics.CheckBox are made at the same frame.
            // Physics.SyncTransforms() updates the Transforms to the physics engine and Physics.CheckBox works
            core.coreGO.SetActive(true);
            Physics.SyncTransforms();
            var randomRotY = UnityEngine.Random.Range(-180f, 180f);
            core.coreGO.transform.rotation = Quaternion.Euler(new Vector3(0, randomRotY, 0));
            core.coreGO.transform.position = GetRandomSpawnPosInArena();
            core.coreRb.velocity = Vector3.zero;
            core.coreRb.angularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Resets the energycores positions and velocities.
    /// </summary>
    void SetEnergyCores()
    {
        var ballShape = (EnergyCoreController.CoreShape)m_ResetParams.GetWithDefault(
            "ball_shape",
            (float)m_AIRobotSettings.energyCoreShape);

        var negCorePrefab = ballShape == EnergyCoreController.CoreShape.Block_0
            ? m_NegativeEnergyCoreBlockPrefab
            : m_NegativeEnergyCoreBallPrefab;
        var posCorePrefab = ballShape == EnergyCoreController.CoreShape.Block_0
            ? m_PositiveEnergyCoreBlockPrefab
            : m_PositiveEnergyCoreBallPrefab;

        var nNegCore = (int)m_ResetParams.GetWithDefault(
            "number_of_negative_energy_cores",
            m_AIRobotSettings.numberOfNegEnergyCores);
        var nPosCore = (int)m_ResetParams.GetWithDefault(
            "number_of_positive_energy_cores",
            m_AIRobotSettings.numberOfPosEnergyCores);

        InitializeEnergyCores(negCorePrefab, m_NegEnergyCores, nNegCore, ballShape);
        InitializeEnergyCores(posCorePrefab, m_PosEnergyCores, nPosCore, ballShape);
    }

    void SetArena()
    {
        var levelNum = (int)m_ResetParams.GetWithDefault("level", m_AIRobotSettings.level);

        if (levelNum == currentLevel) return;

        currentLevel = levelNum;
        for (var i = 0; i < m_Arenas.Count; i++)
        {
            if (i == levelNum)
            {
                m_Arenas[i].SetActive(true);
                m_Ground = Utils.FindGameObjectInChildWithTag(m_Arenas[i], "ground");
                // Get the ground's bounds
                m_AreaBounds = m_Ground.GetComponent<Collider>().bounds;

                // Get the ground renderer so we can change the material when a goal is scored
                m_GroundRenderer = m_Ground.GetComponent<Renderer>();
                // Starting materialv
                m_GroundMaterial = m_GroundRenderer.material;
            }
            else m_Arenas[i].SetActive(false);
        }
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
    #endregion // ======= END PRIVATE FUNCTIONS =======
}
