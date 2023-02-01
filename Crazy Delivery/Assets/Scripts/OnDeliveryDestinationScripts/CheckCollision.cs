using AnimationScripts;
using UnityEngine;

namespace OnDeliveryDestinationScripts
{
    public class CheckCollision : MonoBehaviour
    {
        private ScoreManager _scoreManager;
        private PizzaThrowing _pizzaThrowing;
        
        private void Start()
        {
            _scoreManager = GameObject.Find("Score").GetComponent<ScoreManager>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            _pizzaThrowing.SetCanSpawnPizza(true);
            _pizzaThrowing.EnableGravity();
            Destroy(transform.GetComponent<CheckCollision>());
            Destroy(transform.GetComponent<PizzaMover>());
            if (collision.gameObject.CompareTag("Client"))
            {
                FindClientRootAndMakePhysical(collision.collider.transform);
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

        private void FindClientRootAndMakePhysical(Transform clientBone)
        {
            while (clientBone.name != "XBot")
            {
                clientBone = clientBone.parent;
            }
            
            clientBone.GetComponent<RagDollController>().MakePhysical();
        }
    }
}
