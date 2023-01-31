using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EnvironmentScripts
{
    public class ChunkRoadSpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _chunkPrefabs;
        [SerializeField] private GameObject[] _chunkChain = new GameObject[6];
        [SerializeField] private float _roadLength;
        
        private bool _firstSpawn = true;
        private int _counter = 4;
        private GameObject _road;

        private void Start()
        {
            _road = Instantiate(_chunkPrefabs[Random.Range(0, _chunkPrefabs.Count - 1)], transform.position, Quaternion.identity);
            for (int i = 0; i < _chunkChain.Length - 2; i++)
            {
                if (_chunkChain[i] == null)
                {
                    _chunkChain[i] = _road;
                    break;
                }
            }
        }

        public void Spawn()
        {
            Vector3 position = new Vector3((_road.transform.position.z + _roadLength) * _counter, 0, 0);
            _road = Instantiate(_chunkPrefabs[Random.Range(0, _chunkPrefabs.Count)], position, Quaternion.identity);
            if (_firstSpawn)
            {
                _chunkChain[_chunkChain.Length - 2] = _road;
                _firstSpawn = false;
            }
            else
            {
                _chunkChain[_chunkChain.Length - 1] = _road;
                ChunkRemove();
            }
            _counter++;
        }
        
        private void ChunkRemove()
        {
            Destroy(_chunkChain[0]);
            for (int i = 1; i < _chunkChain.Length; i++)
            {
                _chunkChain[i - 1] = _chunkChain[i];
            }
            _chunkChain[_chunkChain.Length - 1] = null;
        }
    }
}
