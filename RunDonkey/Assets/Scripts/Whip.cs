using UnityEngine;

public class Whip : MonoBehaviour
{
    public Donkey_Movement donkey;

    private void OnTriggerEnter(Collider other)
    {
        if (!gameObject.activeSelf) return;

        if (other.CompareTag("Donkey"))
        {
            donkey.ApplyWhipBoost();
        }
    }
}
