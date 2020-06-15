using UnityEngine;

public class PushBlockSettings : MonoBehaviour
{
    [Space(10)]
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
    public Material goalScoredMaterial;

    /// <summary>
    /// When an agent fails, the ground will turn this material for a few seconds.
    /// </summary>
    public Material failMaterial;

    [Space(10)]
    public float ballType;
    public float level;
    public float rayLength;
    public int maxSteps;

}
