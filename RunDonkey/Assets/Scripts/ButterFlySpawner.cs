using UnityEngine;

public class ButterflySpawner : MonoBehaviour
{
    [Header("Butterfly")]
    [SerializeField] private GameObject butterflyPrefab;
    [SerializeField] private int butterflyCount = 10;

    [Header("Spawn Area")]
    [SerializeField] private Transform centerPoint;
    [SerializeField] private float spawnRadius = 5f;
    [SerializeField] private float spawnY = 1.5f;

    [Header("Animation")]
    [SerializeField] private string animationClipName = "Take 001";

    private void Start()
    {
        SpawnButterflies();
    }

    private void SpawnButterflies()
    {
        if (butterflyPrefab == null)
        {
            Debug.LogError("Butterfly prefab is missing.");
            return;
        }

        Vector3 center = centerPoint != null ? centerPoint.position : transform.position;

        for (int i = 0; i < butterflyCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

            Vector3 spawnPosition = new Vector3(
                center.x + randomCircle.x,
                spawnY,
                center.z + randomCircle.y
            );

            Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            GameObject butterfly = Instantiate(butterflyPrefab, spawnPosition, randomRotation);

            Animation anim = butterfly.GetComponent<Animation>();
            if (anim != null)
            {
                AnimationState state = anim[animationClipName];
                if (state != null)
                {
                    state.time = Random.Range(0f, state.length);
                    anim.Play(animationClipName);
                }
                else
                {
                    Debug.LogWarning($"Animation clip '{animationClipName}' not found on spawned butterfly.");
                }
            }
            else
            {
                Debug.LogWarning("Spawned butterfly does not have an Animation component.");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 center = centerPoint != null ? centerPoint.position : transform.position;
        Vector3 gizmoCenter = new Vector3(center.x, spawnY, center.z);

        Gizmos.DrawWireSphere(gizmoCenter, spawnRadius);
    }
}