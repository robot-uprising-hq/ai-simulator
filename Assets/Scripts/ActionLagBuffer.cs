using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionLagBuffer : MonoBehaviour{
    Queue<float> buffer =  new Queue<float>();

    // Size sets the number of rounds the action gomes late.
    // 0=No lag
    // 1=Action is one step late
    // 2=Action is two steps late
    public int size;
    public void InsertAction(float observation)
    {
        buffer.Enqueue(observation);
    }

    public float GetAction()
    {
        if (size == 0 && buffer.Count > 0) return buffer.Dequeue();

        if (buffer.Count <= size) return buffer.Peek();
        else return buffer.Dequeue();
    }
}
