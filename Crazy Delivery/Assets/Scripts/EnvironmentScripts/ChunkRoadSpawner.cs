using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EnvironmentScripts
{
    public class ChunkRoadSpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _chunkPrefabs;
        [SerializeField] private List<GameObject> _chunkChain;
        [SerializeField] private float _roadLength;
        [SerializeField] private int _numberOfChunksBehindPlayer;

        private int _сhunksBeforeStartRemoving;
        
        private int _multiplier = 4;
        private GameObject _road;

        private void Start()
        {
            _сhunksBeforeStartRemoving = _numberOfChunksBehindPlayer;
            
            CreateChunk(transform.position);
        }

        public void Spawn()
        {
            Vector3 position = new Vector3((_road.transform.position.z + _roadLength) * _multiplier, 0, 0);
            
            CreateChunk(position);
            
            ChunkRemove();
        }

        private void CreateChunk(Vector3 position)
        {
            _road = Instantiate(_chunkPrefabs[Random.Range(0, _chunkPrefabs.Count)], position, Quaternion.identity);
            _chunkChain.Add(_road);
            _multiplier++;
        }
        
        private void ChunkRemove()
        {
            if(_сhunksBeforeStartRemoving > 0)
            {
                _сhunksBeforeStartRemoving--;
                return;
            }
            
            Destroy(_chunkChain[0]);
            _chunkChain.RemoveAt(0);
        }
    }
}
