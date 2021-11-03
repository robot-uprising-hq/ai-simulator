using System;
using UnityEngine;

public class Arena : MonoBehaviour
{

  [HideInInspector]
  public float m_AgentSpeedRandomFactor = 1.0f;
  public virtual void OnEpisodeBegin() {}

  public virtual Vector3 GetSpawnPosInArena(int type = -1) {
    return Vector3.zero;
  }

  public virtual void GoalTouched(AIRobotAgent.Team goalHit, EnergyCoreController.CoreType coreType) {}
}