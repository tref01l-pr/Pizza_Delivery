using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EnvironmentScripts
{
    public class ChunkRoadSpawner : MonoBehaviour
    {
        [SerializeField] private List<Chunk> _chunkPrefabs;
        [SerializeField] private List<Chunk> _chunkChain;
        [SerializeField] private float _roadLength;
        [SerializeField] private int _numberOfChunksBehindPlayer;
        [SerializeField] private ScoreManager _scoreManager;
        [SerializeField] private int _numberOfChunksOnStart;
        [SerializeField] private GameObject _player;
        

        private int _сhunksBeforeStartRemoving;
        private int _multiplier = 0;
        private Chunk _road;

        private void Start()
        {
            _сhunksBeforeStartRemoving = _numberOfChunksBehindPlayer;

            SpawnMapOnStart(_numberOfChunksOnStart);
        }

        private void SpawnMapOnStart(float numberOfChunksOnStart)
        {
            Vector3 startPosition = new Vector3((transform.position.z + _roadLength) * _multiplier, 0, 0);

            CreateChunk(startPosition, 0);
            
            while (numberOfChunksOnStart > 0)
            {
                Vector3 newPosition = new Vector3((transform.position.z + _roadLength) * _multiplier, 0, 0);
                
                CreateChunk(newPosition, Random.Range(1, _chunkPrefabs.Count));
                
                numberOfChunksOnStart--;
            }
        }

        public void Spawn()
        {
            Vector3 position = new Vector3((_road.transform.position.z + _roadLength) * _multiplier, 0, 0);
            
            CreateChunk(position, Random.Range(1, _chunkPrefabs.Count));
            
            ChunkRemove();
        }

        private void CreateChunk(Vector3 position, int numberOfChunk)
        {
            _road = Instantiate(_chunkPrefabs[numberOfChunk], position, Quaternion.identity);
            _chunkChain.Add(_road);
            _road.Initialize(_scoreManager, _player);
            
            _road.Initialize(_player);
            
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
