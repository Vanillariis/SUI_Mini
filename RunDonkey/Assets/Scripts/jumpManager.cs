using UnityEngine;

using System.Collections.Generic;

public class jumpManager : MonoBehaviour
{
    public List<GameObject> jumps = new List<GameObject>(); // Assign jump objects in Inspector
    public GameObject finishline;

    void Start()
    {
        if (finishline != null)
            finishline.SetActive(false);
    }

    public void JumpCompleted(GameObject jump)
    {
        if (jumps.Count == 0) return;

        // Only allow the FIRST jump in the list
        if (jump == jumps[0])
        {
            jumps.RemoveAt(0);
            Debug.Log($"EVENT:JumpCompleted|Name:{jump.name}");
            Destroy(jump);

            if (jumps.Count == 0 && finishline != null)
            {
                finishline.SetActive(true);
            }
        }
        else
        {
            Debug.Log($"EVENT:WrongJump|Expected:{jumps[0].name}|Got:{jump.name}");
            Debug.Log("Wrong jump order!");
        }
    }
}
