using UnityEngine;

namespace OnDeliveryDestinationScripts
{
    public class PlayerPositionController : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        [SerializeField] private Rigidbody[] playerRigidbody;

        private Vector3 _newPos;
        
        public bool IsRiding { get; private set; } = true;
        
        private void Update()
        {
            if (IsRiding == false)
            {
                DisActivateBicycle();
            }
        }

        public void SetNewPos(Vector3 newPos)
        {
            _newPos = newPos;
        }

        public void SetIsRiding(bool isRiding)
        {
            IsRiding = isRiding;
        }
        
        private void TeleportPlayerOnDestination()
        {
            player.transform.position = _newPos;
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
    }
}
