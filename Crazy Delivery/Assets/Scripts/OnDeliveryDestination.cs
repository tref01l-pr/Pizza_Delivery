using Unity.VisualScripting;
using UnityEngine;

public class OnDeliveryDestination : MonoBehaviour
{
    [SerializeField] private PizzaThrowing _pizzaThrowing;
    [SerializeField] private PlayerPositionController _playerPositionController;
    
    [SerializeField] private GameObject playerRoot;
    [SerializeField] private GameObject joystick;
    [SerializeField] private GameObject deliveryDestination;
    [SerializeField] private GameObject listOfClients;
    [SerializeField] private BoxCollider deliveryRoadCollider;

    public Camera deliveryCam;
    private Camera cam;

    public void TurnOnMainCamera()
    {
        TurnOnRoadCollider();
        cam.enabled = true;
        deliveryCam.enabled = false;
    }
    
    private void Start()
    {
        cam = Camera.main;
        cam.enabled = true;
        deliveryCam.enabled = false;
        joystick = GameObject.Find("Fixed Joystick");
        playerRoot = GameObject.Find("PushBikeWRagdoll");
        _playerPositionController = playerRoot.GetComponent<PlayerPositionController>();
    }


    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(deliveryDestination);
            _playerPositionController.newPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            _playerPositionController.isRiding = false;
            _pizzaThrowing.OnDestination = true;
            _pizzaThrowing.pizzaSightSpawn = true;

            FindNumberOfClients();
            TurnOnDeliveryCamera();
        }
    }

    private void TurnOnRoadCollider()
    {
        deliveryRoadCollider.enabled = true;
    }
    
    private void TurnOnDeliveryCamera()
    {
        cam.enabled = false;
        deliveryCam.enabled = true;
    }

    private void FindNumberOfClients()
    {
        _pizzaThrowing.numberOfClients = listOfClients.transform.childCount;
        _pizzaThrowing.numberOfThrowingChance = _pizzaThrowing.numberOfClients * 2;
    }
}
