using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EnvironmentScripts
{
    public class ChunkRoadSpawner : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _chunkPrefabs;
        [SerializeField] private GameObject[] _chunkChain = new GameObject[6]; //убрать прикол сделать List
        [SerializeField] private float _roadLength;
        
       
        private int _counter = 4; // переименовать multiplier
        private GameObject _road; 

        private void Start()
        {
           CreateChunk(transform.positon); // вместо for
           /* for (int i = 0; i < _chunkChain.Length - 2; i++)
            {
                if (_chunkChain[i] == null)
                {
                    _chunkChain[i] = _road; 
                    break;
                }
            }*/
        }

        public void Spawn()
        {
            Vector3 position = new Vector3((_road.transform.position.z + _roadLength) * _counter, 0, 0);
            
           CreateChunk(position);
            
          
       
            ChunkRemove();//OldChunkRemove
          
     
        }
        
        private CreateChunk(Vector3 position)
        {
          _road = Instantiate(_chunkPrefabs[Random.Range(0, _chunkPrefabs.Count)], position, Quaternion.identity);
            _chunkChain.Add(_road); 
            _counter++;
         
        }
        
        private void ChunkRemove()
        {
        //попробуй удалять первый елемент листа и проверить смещается ли лист
            Destroy(_chunkChain[0]);
            for (int i = 1; i < _chunkChain.Length; i++)
            {
                _chunkChain[i - 1] = _chunkChain[i];
            }
            _chunkChain[_chunkChain.Length - 1] = null;
        }
    }
}
