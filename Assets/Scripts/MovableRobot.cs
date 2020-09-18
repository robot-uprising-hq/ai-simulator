using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableRobot : MonoBehaviour
{
    public float maxTorque = 40.0f;
    public List<WheelCollider> leftWheels;
    public List<WheelCollider> rightWheels;

    private Coroutine moveTimeout;

    // Start is called before the first frame update
    void Start()
    {

    }

    // The real robots will have two motors, with distinct values for each motor. Motor
    // can go forward or backward.

    // In addition, the robots should be movable by keyboard with reasonably good
    // suspension of disbelief and physical correspondence to the real thing.

    //
    // speed should be from -1 to 1.
    public void MoveRobot(
        float leftSpeed,
        float rightSpeed,
        float timeout)
    {
        //Debug.Log($"left speed: {leftSpeed} right speed {rightSpeed} timeout {timeout}");
        foreach (WheelCollider coll in leftWheels)
        {
            coll.motorTorque = leftSpeed * maxTorque;
		}
        foreach (WheelCollider coll in rightWheels)
        {
            coll.motorTorque = rightSpeed * maxTorque;
        }

        if (timeout > 0.0)
        {
            if (moveTimeout != null)
            {
                StopCoroutine(moveTimeout);
            }
            moveTimeout = StartCoroutine(StopMoveAfterTimeout(timeout));
        }
    }

    private IEnumerator StopMoveAfterTimeout(float timeout)
    {
        yield return new WaitForSeconds(timeout);
        MoveRobot(0, 0, 0);
        moveTimeout = null;
    }

}