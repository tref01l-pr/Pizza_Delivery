using OnDeliveryDestinationScripts;
using UnityEngine;

namespace CameraScripts
{
    public class CameraFollower : MonoBehaviour
    {
        [SerializeField] private GameObject _target;
        [SerializeField] private PlayerPositionController _playerPositionController;
        [SerializeField] private float _targetX;
        [SerializeField] private float _targetZ;
        
        private void Update()
        {
            if(PlayerRiding)
                Follow();
        }

        private bool PlayerRiding => _playerPositionController.IsRiding;

        private void Follow()
        {
            Vector3 newPosition = new Vector3(_target.transform.position.x - _targetX, transform.position.y, _target.transform.position.z - _targetZ);
            transform.position = newPosition;
        }
    }
}
