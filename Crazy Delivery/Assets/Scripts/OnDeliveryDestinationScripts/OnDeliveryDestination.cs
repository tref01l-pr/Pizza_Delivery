using UnityEngine;

namespace OnDeliveryDestinationScripts
{
    public class OnDeliveryDestination : MonoBehaviour
    {
        [SerializeField] private PizzaThrowing _pizzaThrowing;
        [SerializeField] private PlayerPositionController _playerPositionController;
    
        private GameObject _playerRoot;
        [SerializeField] private GameObject _listOfClients;
        [SerializeField] private BoxCollider _deliveryRoadCollider;

        [SerializeField] private Camera _deliveryCam;
        private Camera _cam;

        public void TurnOnMainCamera()
        {
            TurnOnRoadCollider();
            _cam.enabled = true;
            _deliveryCam.enabled = false;
        }
    
        private void Start()
        {
            _cam = Camera.main;
            if (_cam != null) _cam.enabled = true;
            _deliveryCam.enabled = false;
            _playerRoot = GameObject.Find("PushBikeWRagdoll");
            _playerPositionController = _playerRoot.GetComponent<PlayerPositionController>();
        }


        private void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Destroy(gameObject);
                _playerPositionController.NewPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                _playerPositionController.IsRiding = false;
                _pizzaThrowing.OnDestination = true;
                _pizzaThrowing.PizzaSightSpawn = true;

                FindNumberOfClients();
                TurnOnDeliveryCamera();
            }
        }

        private void TurnOnRoadCollider()
        {
            _deliveryRoadCollider.enabled = true;
        }
    
        private void TurnOnDeliveryCamera()
        {
            _cam.enabled = false;
            _deliveryCam.enabled = true;
        }

        private void FindNumberOfClients()
        {
            _pizzaThrowing.NumberOfClients = _listOfClients.transform.childCount;
            _pizzaThrowing.NumberOfThrowingChance = _pizzaThrowing.NumberOfClients * 2;
        }
    }
}
