using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class GameScene : MonoBehaviour
{
    GameArena[] arenas;
    public int defaultNumArenas = 0;

    public void Awake()
    {
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
        arenas = GetComponentsInChildren<GameArena>(true);

        EnvironmentReset();
    }

    void EnvironmentReset()
    {
        int numArenas = (int)Academy.Instance.EnvironmentParameters.GetWithDefault("max_arenas", defaultNumArenas);
        int ind = 0;
        if (numArenas == 0)
        {
            // Leave them as they were.
            return;
        }

        foreach (GameArena obj in arenas)
        {
            obj.gameObject.SetActive(ind < numArenas);
            ind++;
        }
    }

}
