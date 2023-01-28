using UnityEngine;

namespace EnvironmentScripts
{
    public class CarRiding : MonoBehaviour
    {
        private TrafficSpawnerAndDestroyer _trafficSpawnerAndDestroyer;

        private  void Start()
        {
            _trafficSpawnerAndDestroyer = transform.parent.GetComponent<TrafficSpawnerAndDestroyer>();
        }
    
        private void Update()
        {
            transform.localPosition -= transform.forward * _trafficSpawnerAndDestroyer.SpeedOfCarRidingForCarRiding * Time.deltaTime;
        }
    }
}
