using OnDeliveryDestinationScripts;
using UnityEngine;

namespace CameraScripts
{
    public class CameraFollower : MonoBehaviour
    {
        [SerializeField] private GameObject _player;
        [SerializeField] private PlayerPositionController _playerPositionController;

        private void Update()
        {
            if(_playerPositionController.IsRiding)
                camFollowPlayer();
        }

        private void camFollowPlayer()
        {
            Vector3 newPos = new Vector3(_player.transform.position.x - 19f, transform.position.y, _player.transform.position.z - 16f);
            transform.position = newPos;
        }
    }
}
