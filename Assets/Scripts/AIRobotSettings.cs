using UnityEngine;
using UnityEditor;
using Unity.MLAgents;

public class AIRobotSettings : MonoBehaviour
{
    [Space(10)]
    [Header("Number of items in the arena")]
    public int numberOfBlueAgents;
    public int numberOfRedAgents;
    public int numberOfPosEnergyCores;
    public int numberOfNegEnergyCores;

    [Space(10)]
    [Header("Rewards")]
    public int negECoreHitsOwnGoalReward;
    public int negECoreHitsOpponentGoalReward;

    public int posECoreHitsOwnGoalReward;

    public int posECoreHitsOpponentGoalReward;

    [Space(10)]
    [Header("Agent movement settings")]
    /// <summary>
    /// The "walking speed" of the agents in the scene.
    /// </summary>
    public float agentRunSpeed;
    // Value is a percent of original which is added or substracted from original
    public float agentRunSpeedRandom;

    [Space(10)]
    /// <summary>
    /// The agent rotation speed.
    /// Every agent will use this setting.
    /// </summary>
    public float agentRotationSpeed;
    // Value is a percent of original which is added or substracted from original
    public float agentRotationSpeedRandom;

    [Space(10)]
    public float agentMoveRotMoveSpeed;
    public float agentMoveRotTurnSpeed;

    [Space(10)]
    // Value is a percent of original which is added or substracted from original
    public float observationDistanceRandom;
    // Value is degrees which is added or substracted from original
    public float observationAngleRandom;

    [Space(10)]
    [Header("Arena settings")]
    /// <summary>
    /// The spawn area margin multiplier.
    /// ex: .9 means 90% of spawn area will be used.
    /// .1 margin will be left (so players don't spawn off of the edge).
    /// The higher this value, the longer training time required.
    /// </summary>
    public float spawnAreaMarginMultiplier;

    /// <summary>
    /// When a goal is scored the ground will switch to this
    /// material for a few seconds.
    /// </summary>
    public Material blueTeamSuccessMat;

    /// <summary>
    /// When an agent fails, the ground will turn this material for a few seconds.
    /// </summary>
    public Material redTeamSuccessMat;

    [Space(10)]
    [Header("Game settings")]
    public EnergyCoreController.CoreShape energyCoreShape;
    public float level;
    public float rayLength;

}
