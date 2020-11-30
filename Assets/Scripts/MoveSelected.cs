using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSelected : MonoBehaviour
{
    public List<MovableRobot> gameObjects;
    public float steerAmount = 0.01f;
    public float torqueAmount = 0.1f;

    public KeyCode changeRobotKey = KeyCode.Tab;
    public KeyCode robotForwardKey = KeyCode.UpArrow;
    public KeyCode robotBackwardKey = KeyCode.DownArrow;
    public KeyCode robotLeftKey = KeyCode.LeftArrow;
    public KeyCode robotRightKey = KeyCode.RightArrow;

    
    MovableRobot selected;
    public float steering = 0;
    public float torque = 0;
  
    public int currentSelectedIndex = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        selected = gameObjects[0];
    }

    void NextRobot()
    {
        currentSelectedIndex += 1;
        currentSelectedIndex %= gameObjects.Count;
        // cut down power to robot that is no longer being moved.
        selected.MoveRobot(0, 0, 0);
        selected = gameObjects[currentSelectedIndex];
        // Start from empty for new robot.
        torque = 0;
        steering = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(changeRobotKey))
        {
            NextRobot();
        }

        float torqueDelta = 0;

        if (Input.GetKey(robotForwardKey))
        {
            torqueDelta += torqueAmount;
        }

        if (Input.GetKey(robotBackwardKey))
        {
            torqueDelta -= torqueAmount;
        }

        if (torqueDelta == 0)
        {
            if (Mathf.Abs(torque) < torqueAmount)
            {
                torque = 0;
            }
            else
            {
                torque += -Mathf.Sign(torque) * torqueAmount;
            }
        }
        else
        {
            torque += torqueDelta;
        }

        // Bounds check.
        if (torque < -1)
        {
            torque = -1;
        }
        else if (torque > 1)
        {
            torque = 1;
        }

        float steerDelta = 0;
        if (Input.GetKey(robotLeftKey))
        {
            steerDelta += steerAmount;
        }
        if (Input.GetKey(robotRightKey))
        {
            steerDelta -= steerAmount;
        }

        if (steerDelta == 0)
        {
            steering += -Mathf.Sign(steering) * steerAmount;
        }
        else
        {
            steering += steerDelta;
        }

        // Bounds check.
        if (steering < -1)
        {
            steering = -1;
        }
        else if (steering > 1)
        {
            steering = 1;
        }

        float leftSpeed, rightSpeed;
        leftSpeed = ((-1 + steering) / -2) * torque;
        rightSpeed = ((1 + steering) / 2) * torque;
        selected.MoveRobot(leftSpeed, rightSpeed, 0);
    }
}
