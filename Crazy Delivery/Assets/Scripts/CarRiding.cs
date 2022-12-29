using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarRiding : MonoBehaviour
{
    [SerializeField] private TrafficSpawnerAndDestroyer _trafficSpawnerAndDestroyer;

    void Start()
    {
        _trafficSpawnerAndDestroyer = transform.parent.GetComponent<TrafficSpawnerAndDestroyer>();
    }
    
    void FixedUpdate()
    {
        transform.localPosition -= transform.forward * _trafficSpawnerAndDestroyer.speedOfCarRidingForCarRiding * Time.deltaTime;
    }
    
}
