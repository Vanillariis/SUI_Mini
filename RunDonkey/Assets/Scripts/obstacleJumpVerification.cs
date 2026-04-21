using UnityEngine;
using System.Collections;

public class obstacleJumpVerification : MonoBehaviour
{
    int jumpStage = 0;

    public GameObject finishline;
    public jumpManager jumpManager;

    void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("takeoff") && jumpStage == 0)
        {
            print("Takeoff");
            jumpStage = 1;
        }

        else if (col.CompareTag("hover") && jumpStage == 1)
        {
            print("Hover");
            jumpStage = 2;
        }

        else if (col.CompareTag("landing") && jumpStage == 2)
        {
            print("Jump completed");

            if (jumpManager != null)
            {
            jumpManager.JumpCompleted(col.transform.root.gameObject);
            }

            jumpStage = 0;
        }

        if (col.CompareTag("finishline"))
        {
            print("success");
            if (finishline != null)
                finishline.SetActive(false);
        }   
    }
}