
using UnityEngine;

public class EnergyCoreController : MonoBehaviour
{
    public enum CoreShape
    {
        Block_0 = 0,
        Ball_1 = 1
    }

    public enum CoreType
    {
        Negative = 0,
        Positive = 1
    }

    [HideInInspector]
    public Arena arena;
    public CoreType m_CoreType;

    public string redGoalTag; //will be used to check if collided with red goal
    public string blueGoalTag; //will be used to check if collided with blue goal

    void Awake()
    {
        arena = transform.parent.GetComponent<GameArena>();
        if (arena == null) {
            arena = transform.parent.GetComponent<GameArenaWithHuman>();
        }
    }

    void Update()
    {
        // Check for occasional energy core getting out of arena
        // and dropping down.
        if (transform.position.y < -0.5f)
        {
            transform.position = arena.GetSpawnPosInArena();
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag(redGoalTag)) //ball touched red goal
        {
            gameObject.SetActive(false);
            arena.GoalTouched(AIRobotAgent.Team.Red, m_CoreType);
        }
        if (col.gameObject.CompareTag(blueGoalTag)) //ball touched blue goal
        {
            gameObject.SetActive(false);
            arena.GoalTouched(AIRobotAgent.Team.Blue, m_CoreType);
        }
    }
}
