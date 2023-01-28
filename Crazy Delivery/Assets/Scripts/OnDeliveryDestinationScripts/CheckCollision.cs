using AnimationScripts;
using UnityEngine;

namespace OnDeliveryDestinationScripts
{
    public class CheckCollision : MonoBehaviour
    {
        [SerializeField] private ScoreManager scoreManager;
        private PizzaThrowing _pizzaThrowing;
    
        private Transform _verification;

        public void Init(PizzaThrowing pizzaThrowing)
        {
            _pizzaThrowing = pizzaThrowing;
        }
    
        private void Start()
        {
            scoreManager = GameObject.Find("Score").GetComponent<ScoreManager>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            _pizzaThrowing.canSpawnPizza = true;
            _pizzaThrowing.isPizza = false;
            _pizzaThrowing.EnableGravity();
            Destroy(_pizzaThrowing.pizzaSpawn.GetComponent<CheckCollision>());
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
                _pizzaThrowing.numberOfClients--;
                scoreManager.AddPoint();
                Destroy(collision);
            }
        }
    }
}
