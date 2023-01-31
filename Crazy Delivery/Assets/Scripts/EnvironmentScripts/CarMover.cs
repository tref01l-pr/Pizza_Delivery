using UnityEngine;

namespace EnvironmentScripts
{
    public class CarMover : MonoBehaviour
    {
        private TrafficSpawnerAndDestroyer _trafficSpawnerAndDestroyer;

        private  void Start()
        {
            _trafficSpawnerAndDestroyer = transform.parent.GetComponent<TrafficSpawnerAndDestroyer>();
        }
    
        private void Update()
        {
            Move();
        }

        private void Move()
        {
            transform.localPosition -= transform.forward * _trafficSpawnerAndDestroyer.CarSpeedForCarRiding * Time.fixedDeltaTime;
        }
        
    }
}
