using AnimationScripts;
using UnityEngine;

namespace OnDeliveryDestinationScripts
{
    public class CheckCollision : MonoBehaviour
    {
        private ScoreManager _scoreManager;
        private PizzaThrowing _pizzaThrowing;
        private Transform _verification;
        
        private void Start()
        {
            _scoreManager = GameObject.Find("Score").GetComponent<ScoreManager>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            _pizzaThrowing.SetCanSpawnPizza(true);
            _pizzaThrowing.SetIsPizza(false);
            _pizzaThrowing.EnableGravity();
            Destroy(transform.GetComponent<CheckCollision>());
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
                _pizzaThrowing.SetNumberOfClients(_pizzaThrowing.NumberOfClients - 1);
                _scoreManager.AddPoint();
                Destroy(collision);
            }
        }
        
        public void Init(PizzaThrowing pizzaThrowing)
        {
            _pizzaThrowing = pizzaThrowing;
        }
    }
}
