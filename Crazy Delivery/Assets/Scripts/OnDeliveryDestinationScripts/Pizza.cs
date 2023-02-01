using UnityEngine;

namespace OnDeliveryDestinationScripts
{
    public class Pizza : MonoBehaviour
    {
        [SerializeField] private CollisionObserver _collisionObserver;
        [SerializeField] private Collider _collider;
        
        public void Initialize(PizzaThrowing pizzaThrowing, ScoreManager scoreManager)
        {
            _collisionObserver.Initialize(pizzaThrowing,scoreManager);
            _collider.enabled = true;
        }
    }
}
