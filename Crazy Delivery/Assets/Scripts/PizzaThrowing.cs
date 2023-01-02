using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PizzaThrowing : MonoBehaviour
{
    [HideInInspector] public bool OnDestination { get; set; } = false; 
    [HideInInspector] public bool isPizza { get; set; } = false;
    [HideInInspector] public bool canSpawnPizza { get; set; } = true;
    public int numberOfClients;
    public int numberOfThrowingChance;
    
    [HideInInspector] public GameObject pizzaSpawn;
    public bool pizzaSightSpawn { get; set; } = false;
    [SerializeField] private GameObject pizza;
    [SerializeField] private GameObject pizzaSight;
    [SerializeField] private GameObject playerRoot;
    [SerializeField] private MeshRenderer pizzaSightMeshRenderer;
    private Vector3 spawnPos;
    
    [SerializeField] private OnDeliveryDestination _onDeliveryDestination;
    [SerializeField] private PlayerPositionController _playerPositionController;
    [SerializeField] private ScoreManager _scoreManager;
    
    [SerializeField] private float speedRotationSight = 3;
    [SerializeField] private float speedOfFlying = 30;
    private float speedErroreEclusion;
    [SerializeField] private bool rightSide;

    public void EnableGravity()
    {
        pizzaSpawn.GetComponent<Rigidbody>().useGravity = true;
    }

    private void Start()
    {
        _scoreManager = GameObject.Find("Score").GetComponent<ScoreManager>();
        speedErroreEclusion = speedRotationSight * 2;
        playerRoot = GameObject.Find("PushBikeWRagdoll");
        _playerPositionController = playerRoot.GetComponent<PlayerPositionController>();
        _playerPositionController.isRiding = true;
        spawnPos = _onDeliveryDestination.deliveryCam.transform.position;
        spawnPos.y = spawnPos.y - 0.5f;
        pizzaSightMeshRenderer.enabled = false;
    }

    private void Update()
    {
        if (OnDestination)
        {
            SightRotation();
            
            if (canSpawnPizza)
            {
                GetInput();
            }
       
            if (numberOfClients == 0 || numberOfThrowingChance < 0)
            {
                _playerPositionController.isRiding = true;
                EndPizzaThrowing();
            }
        }
    }

    private void FixedUpdate()
    {
        if (OnDestination)
        {
            if (isPizza)
            {
                PizzaFly();
            }
        }
    }
    

    private void GetInput()
    {
        if (Input.GetMouseButtonDown(0))
        { 
            numberOfThrowingChance--;
            canSpawnPizza = false;
            if (numberOfClients > 0 && numberOfThrowingChance >= 0)
            {
                pizzaSpawn = Instantiate(pizza, spawnPos, 
                    Quaternion.Euler(0, pizzaSight.transform.localEulerAngles.y, 0), transform.root);
                pizzaSpawn.GetComponent<CheckCollision>().Init(this);
                isPizza = true;
                pizzaSpawn.GetComponent<Collider>().enabled = true;
            }
        }
    }
    
    private void PizzaFly()
    {
        if (rightSide)
        {
            pizzaSpawn.transform.localPosition += pizzaSpawn.transform.right * speedOfFlying * Time.deltaTime;
        }
        else
        {
            pizzaSpawn.transform.localPosition -= pizzaSpawn.transform.right * speedOfFlying * Time.deltaTime;
        }
    }
    
    private void SightRotation()
    {
        if (pizzaSightSpawn)
        {
            pizzaSight.transform.localEulerAngles =
                new Vector3(pizzaSight.transform.localEulerAngles.x, 90, pizzaSight.transform.localEulerAngles.z);
            pizzaSightMeshRenderer.enabled = true;
            pizzaSightSpawn = false;
        } 
        
        if (pizzaSight.transform.localEulerAngles.y < 75f || pizzaSight.transform.localEulerAngles.y > 105f)
        {
            speedRotationSight = -speedRotationSight;
            speedErroreEclusion = -speedErroreEclusion;
            pizzaSight.transform.Rotate(Vector3.up * speedErroreEclusion * Time.deltaTime);
        }
        
        
        pizzaSight.transform.Rotate(Vector3.up * speedRotationSight * Time.deltaTime);
    }

    private void EndPizzaThrowing()
    {
        while (numberOfThrowingChance > 0)
        {
            _scoreManager.AddPoint();
            numberOfThrowingChance--;
        }
        OnDestination = false;
        pizzaSightMeshRenderer.enabled = false;
        _onDeliveryDestination.TurnOnMainCamera();
    }
}
