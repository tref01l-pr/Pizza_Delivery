using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EnvironmentScripts
{
    public class TrafficSpawnerAndDestroyer : MonoBehaviour
    {
        [SerializeField] private List<GameObject> _trafficPreffabs;
        [SerializeField] private float _spawningDelay;
        [SerializeField] private float _carSpeed;

        private float _distnceToSpawner = 20f;
        private bool _canSpawn = true;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;

        public float CarSpeedForCarRiding => _carSpeed;
        
        private void Start()
        {
            _spawnPosition = transform.position;
            if (transform.position.z > 0)
            {
                _spawnPosition.z -= _distnceToSpawner;
            }
            else
            {
                _spawnPosition.z += _distnceToSpawner;
            }
        }

        private void Update()
        {
            if (_canSpawn)
            {
                _canSpawn = false;
                StartCoroutine(SpawningDelay());
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Car"))
            {
                Destroy(collision.gameObject);
            }
        }

        private void CreateCar(Quaternion rotation)
        {
            Instantiate(_trafficPreffabs[Random.Range(0, _trafficPreffabs.Count - 1)], _spawnPosition, rotation, transform);
        }
        
        private IEnumerator SpawningDelay()
        {
            yield return new WaitForSeconds(_spawningDelay);
            if (transform.position.z > 0)
            {
                CreateCar(Quaternion.Euler(0f, 180f, 0f));
            }
            else
            {
                CreateCar(Quaternion.Euler(0f, 0f, 0f));
            }
            
            _canSpawn = true;
        }
    }
}
