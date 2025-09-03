using AnimationScripts;
using UnityEngine;

namespace OnDeliveryDestinationScripts
{
    public class CollisionObserver : MonoBehaviour
    {
        [SerializeField] private PizzaThrowing _pizzaThrowing;
        
        private ScoreManager _scoreManager;

        private void OnCollisionEnter(Collision collision)
        {
            _pizzaThrowing.SetCanSpawnPizza(true);
            _pizzaThrowing.EnableGravity();
            Destroy(transform.GetComponent<CollisionObserver>());
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
                
            }
        }
        
        public void Initialize(PizzaThrowing pizzaThrowing, ScoreManager scoreManager)
        {
            _pizzaThrowing = pizzaThrowing;
            _scoreManager = scoreManager;
        }

        private void FindClientRootAndMakePhysical(Transform clientBone)
        {
            while (clientBone.name != "XBot")
            {
                clientBone = clientBone.parent;
            }
            var isPhysical = clientBone.GetComponent<RagDollController>();
            if (!isPhysical.IsPhysical)
            {
                isPhysical.MakePhysical();
                _pizzaThrowing.SetNumberOfClients(_pizzaThrowing.NumberOfClients - 1);
                _scoreManager.AddPoint();
            }
        }
    }
}
