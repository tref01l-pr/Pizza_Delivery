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
        [SerializeField] private float _speedOfCarRiding; // переименовить carSpeed
        
        private bool _canSpawn = true;
        private Vector3 _spawnPos;// position
        private Quaternion _spawnRotation;
    
        public float SpeedOfCarRidingForCarRiding { get; private set; } // исправить на то что ниже
        
        public float SpeedOfCarRidingForCarRiding => _speedOfCarRiding;    
        
        private void Start()
        {
            //SpeedOfCarRidingForCarRiding = _speedOfCarRiding;
            _spawnPos = transform.position;
            if (transform.position.z > 0)
            {
                _spawnPos.z -= 20; // убрать магическое число
            }
            else
            {
                _spawnPos.z += 20;
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
            Instantiate(_trafficPreffabs[Random.Range(0, _trafficPreffabs.Count - 1)], _spawnPos, rotation, transform);
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
