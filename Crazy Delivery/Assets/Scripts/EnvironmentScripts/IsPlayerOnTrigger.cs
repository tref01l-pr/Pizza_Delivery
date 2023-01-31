using UnityEngine;

namespace EnvironmentScripts
{
    public class IsPlayerOnTrigger : MonoBehaviour // trigger chunk observer переименовать
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
}
