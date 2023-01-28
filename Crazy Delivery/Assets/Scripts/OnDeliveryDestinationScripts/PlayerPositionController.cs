using UnityEngine;

namespace OnDeliveryDestinationScripts
{
    public class PlayerPositionController : MonoBehaviour
    {
        public Vector3 NewPos { get; set; }
        public bool IsRiding { get; set; } = true;

        [SerializeField] private GameObject player;
        [SerializeField] private Rigidbody[] playerRigidbody;

        private void TeleportPlayerOnDestination()
        {
            player.transform.position = NewPos;
        }

        private void DisActivateBicycle()
        {
            for (int i = 0; i < playerRigidbody.Length; i++) 
            { 
                playerRigidbody[i].velocity = Vector3.zero; 
                playerRigidbody[i].angularVelocity = Vector3.zero;
            }
            player.transform.eulerAngles = new Vector3(0f, 90f, 0f);

            TeleportPlayerOnDestination();
        }
    
        private void Update()
        {
            if (IsRiding == false)
            {
                DisActivateBicycle();
            }
        }
    }
}
