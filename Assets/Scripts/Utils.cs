using UnityEngine;

public class Utils
{
    public static GameObject FindGameObjectInChildWithTag (GameObject parent, string tag)
     {
         Transform t = parent.transform;

         for (int i = 0; i < t.childCount; i++) 
         {
             if(t.GetChild(i).gameObject.tag == tag)
             {
                 return t.GetChild(i).gameObject;
             }
                 
         }
         return null;
     }

    public static float AddRandomFactor(float randomFactor)
    {
        float randomMultiplier = Random.Range(-randomFactor, randomFactor);
        return 1 + randomMultiplier;
    }
}
