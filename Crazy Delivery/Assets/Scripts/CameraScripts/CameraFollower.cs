using OnDeliveryDestinationScripts;
using UnityEngine;

namespace CameraScripts
{
    public class CameraFollower : MonoBehaviour
    {
        [SerializeField] private GameObject _player; // _target
        [SerializeField] private PlayerPositionController _playerPositionController;
        
        private void Update()
        {
        //Метод с большой буквы
            if(PlayerRiding) // 1 =>
                camFollowPlayer();
        }
        
        //1 +>
        private bool PlayerRiding => _playerPositionController.IsRiding;

        private void camFollowPlayer() //Название метод глагол например просто Follow
        {
        // именования без сокращений, даже таких очевидных Position
            Vector3 newPos = new Vector3(_player.transform.position.x - 19f, transform.position.y, _player.transform.position.z - 16f);// магические числа ????
            transform.position = newPos;
        }
    }
}
