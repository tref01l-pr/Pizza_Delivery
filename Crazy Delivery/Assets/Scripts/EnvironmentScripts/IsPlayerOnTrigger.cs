using UnityEngine;

public class IsPlayerOnTrigger : MonoBehaviour
{
    [SerializeField] ChunkRoadSpawner _chunkRoadSpawner;
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("ChunkTrigger"))
        {
            _chunkRoadSpawner.Spawn();
        }
    }
}
