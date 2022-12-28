using System;
using UnityEngine;

public class CheckCollision : MonoBehaviour
{
    [SerializeField] private ScoreManager _scoreManager;
    private PizzaThrowing _pizzaThrowing;
    
    private Transform verification;

    public void Init(PizzaThrowing pizzaThrowing)
    {
        _pizzaThrowing = pizzaThrowing;
    }
    
    private void Start()
    {
        _scoreManager = GameObject.Find("Score").GetComponent<ScoreManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        _pizzaThrowing.canSpawnPizza = true;
        _pizzaThrowing.isPizza = false;
        _pizzaThrowing.EnableGravity();
        Destroy(_pizzaThrowing.pizzaSpawn.GetComponent<CheckCollision>());
        if (collision.gameObject.CompareTag("Client"))
        {
            verification = collision.collider.transform;
            while (verification.name != "XBot")
            {
                verification = verification.parent;
            }
            verification.GetComponent<RagDollController>().MakePhysical();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("ClientTrigger"))
        {
            _pizzaThrowing.numberOfClients--;
            _scoreManager.AddPoint();
            Destroy(collision);
        }
    }
}
