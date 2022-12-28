using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private GameObject playerRoot;
    private Camera cam;
    
    [SerializeField] private PlayerPositionController _playerPositionController;
    
    [SerializeField] private bool followPlayer = true;

    private void Start()
    {
        cam = Camera.main;
        playerRoot = GameObject.Find("PushBikeWRagdoll");
        _playerPositionController = playerRoot.GetComponent<PlayerPositionController>();
    }

    private void Update()
    {
        if(_playerPositionController.isRiding)
        camFollowPlayer();
    }

    private void camFollowPlayer()
    {
        Vector3 newPos = new Vector3(player.transform.position.x - 19f, transform.position.y, player.transform.position.z - 16f);
        transform.position = newPos;
    }
}
