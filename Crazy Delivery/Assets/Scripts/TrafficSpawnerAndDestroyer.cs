using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TrafficSpawnerAndDestroyer : MonoBehaviour
{
    [HideInInspector] public float speedOfCarRidingForCarRiding { get; set; }
    
    [SerializeField] private List<GameObject> trafficPreffabs;
    private GameObject car;
    
    private Vector3 spawnPos;
    private Quaternion spawnRotation;
    
    [SerializeField] private float spawningDelay;
    [SerializeField] private float speedOfCarRiding;
    
    [SerializeField] private bool rightSide;
    private bool canSpawn = true;
    
    private void Start()
    {
        speedOfCarRidingForCarRiding = speedOfCarRiding;
        spawnPos = transform.position;
        if (rightSide)
        {
            spawnPos.z = spawnPos.z + 20;
        }
        else
        {
            spawnPos.z = spawnPos.z - 20;
        }
    }

    private void Update()
    {
        if (canSpawn)
        {
            canSpawn = false;
            StartCoroutine(SpawningDelay());
        }
    }
    
    private IEnumerator SpawningDelay()
    {
        yield return new WaitForSeconds(spawningDelay);
        if (rightSide != true)
        {
            car = Instantiate(trafficPreffabs[Random.Range(0, trafficPreffabs.Count)], spawnPos, Quaternion.Euler(0f, 180f, 0f), transform);
        }
        else
        {
            car = Instantiate(trafficPreffabs[Random.Range(0, trafficPreffabs.Count)], spawnPos, Quaternion.Euler(0f, 0f, 0f), transform);
        }
        canSpawn = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Car"))
        {
            Destroy(collision.gameObject);
        }
    }
}
