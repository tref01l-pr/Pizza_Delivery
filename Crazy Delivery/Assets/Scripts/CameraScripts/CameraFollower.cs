using UnityEngine;

namespace CameraScripts
{
    public class CameraFollower : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        private GameObject _playerRoot;
        private Camera _cam;
    
        [SerializeField] private PlayerPositionController playerPositionController;
    
        [SerializeField] private bool followPlayer = true;

        private void Start()
        {
            _cam = Camera.main;
            _playerRoot = GameObject.Find("PushBikeWRagdoll");
            playerPositionController = _playerRoot.GetComponent<PlayerPositionController>();
        }

        private void Update()
        {
            if(playerPositionController.isRiding)
                camFollowPlayer();
        }

        private void camFollowPlayer()
        {
            Vector3 newPos = new Vector3(player.transform.position.x - 19f, transform.position.y, player.transform.position.z - 16f);
            transform.position = newPos;
        }
    }
}
