using UnityEngine;

public class Whip : MonoBehaviour
{
    public Donkey_Movement donkey;
    
    [Header("Audio")]
    public AudioClip whipSound;
    public AudioSource whipAudio;

    private void OnTriggerEnter(Collider other)
    {
        if (!gameObject.activeSelf) return;

        if (other.CompareTag("Donkey"))
        {
            whipAudio.PlayOneShot(whipSound);
            
            donkey.ApplyWhipBoost();
        }
    }
}
