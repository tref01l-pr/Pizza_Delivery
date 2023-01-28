using AnimationScripts;
using UnityEngine;

namespace OnDeliveryDestinationScripts
{
    public class CheckCollision : MonoBehaviour
    {
        private ScoreManager _scoreManager;
        private PizzaThrowing _pizzaThrowing;
    
        private Transform _verification;

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
            _pizzaThrowing.CanSpawnPizza = true;
            _pizzaThrowing.IsPizza = false;
            _pizzaThrowing.EnableGravity();
            Destroy(_pizzaThrowing.PizzaSpawn.GetComponent<CheckCollision>());
            if (collision.gameObject.CompareTag("Client"))
            {
                _verification = collision.collider.transform;
                while (_verification.name != "XBot")
                {
                    _verification = _verification.parent;
                }
                _verification.GetComponent<RagDollController>().MakePhysical();
            }
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.CompareTag("ClientTrigger"))
            {
                _pizzaThrowing.NumberOfClients--;
                _scoreManager.AddPoint();
                Destroy(collision);
            }
        }
    }
}
