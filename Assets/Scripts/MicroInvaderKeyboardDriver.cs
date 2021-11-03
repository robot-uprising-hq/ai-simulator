using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicroInvaderKeyboardDriver : MonoBehaviour
{
    AIRobotSettings m_AIRobotSettings;
    // GameObject m_ArenaGO;
    // GameArena m_GameArena;
    Rigidbody m_AgentRb;
    private float m_AgentSpeed;
    private float m_RotationSpeed;

    private float m_AgentMoveRotMoveSpeed;
    private float m_AgentMoveRotTurnSpeed;

     void Awake()
    {
        m_AgentRb = GetComponent<Rigidbody>();

        m_AIRobotSettings = FindObjectOfType<AIRobotSettings>();
        // m_ArenaGO = transform.parent.gameObject;
        // m_GameArena = m_ArenaGO.GetComponent<GameArena>();

        m_AgentSpeed = m_AIRobotSettings.agentRunSpeed;
        m_RotationSpeed = m_AIRobotSettings.agentRotationSpeed;
        m_AgentMoveRotMoveSpeed =  m_AIRobotSettings.agentMoveRotMoveSpeed;
        m_AgentMoveRotTurnSpeed = m_AIRobotSettings.agentMoveRotTurnSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        int action = 0; // Do nothing

        if (Input.GetKey(KeyCode.D))
        {
            action= 3; // Turn right
        }
        else if (Input.GetKey(KeyCode.W))
        {
            action = 1; // Go forward
        }
        else if (Input.GetKey(KeyCode.A))
        {
            action = 4; // Turn left
        }
        else if (Input.GetKey(KeyCode.S))
        {
            action = 2; // Go backward
        }
        else if (Input.GetKey(KeyCode.E))
        {
            action = 5; // Go forward and turn right
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            action = 6; // Go forward and turn left
        }
        MoveRobot(action);
    }

    void MoveRobot (int action) {

        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var agentSpeed = m_AgentSpeed;
        var rotationSpeed = m_RotationSpeed;

        switch (action)
        {
            case 0: // Do nothing
            //     transform.Rotate(Vector3.zero, 0.0f);
            //     m_AgentRb.velocity = Vector3.zero;
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

        // Set agent rotation
        transform.Rotate(
            rotateDir,
            Time.fixedDeltaTime * rotationSpeed * 0.3f);
        // Set agent speed
        m_AgentRb.AddForce(dirToGo * agentSpeed * 0.25f,
            ForceMode.VelocityChange);
    }
}
