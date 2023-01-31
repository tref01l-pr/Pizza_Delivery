using UnityEngine;

namespace EnvironmentScripts
{
    public class CarRiding : MonoBehaviour //CarMover
    {
        private TrafficSpawnerAndDestroyer _trafficSpawnerAndDestroyer;

        private  void Start()
        {
            _trafficSpawnerAndDestroyer = transform.parent.GetComponent<TrafficSpawnerAndDestroyer>();
        }
    
        private void Update()
        {
            // убрать зависимость скорости автомобиля от ФПС
            transform.localPosition -= transform.forward * _trafficSpawnerAndDestroyer.SpeedOfCarRidingForCarRiding * Time.deltaTime; // вынести в метод Move
        }
    }
}
