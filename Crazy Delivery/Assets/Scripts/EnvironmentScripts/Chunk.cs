using OnDeliveryDestinationScripts;
using UnityEngine;

namespace EnvironmentScripts
{
    public class Chunk : MonoBehaviour
    {
        [SerializeField] private PizzaThrowing[] _pizzaThrowing;
        [SerializeField] private OnDeliveryDestination[] _onDeliveryDestinations;
    
        public void Initialize(ScoreManager scoreManager, GameObject player)
        {
            foreach (var pizzaSpawn in _pizzaThrowing)
            {
                pizzaSpawn.Initialize(scoreManager, player);
            }
        }

        public void Initialize(GameObject player)
        {
            foreach (var deliveryDestinations in _onDeliveryDestinations)
            {
                deliveryDestinations.Initialize(player);
            }
        }
    }
}
