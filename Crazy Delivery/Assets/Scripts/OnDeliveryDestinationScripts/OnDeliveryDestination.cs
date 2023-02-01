using UnityEngine;

namespace OnDeliveryDestinationScripts
{
    public class OnDeliveryDestination : MonoBehaviour
    {
        [SerializeField] private PizzaThrowing _pizzaThrowing;
        [SerializeField] private PlayerPositionController _playerPositionController;
        [SerializeField] private GameObject _listOfClients;
        [SerializeField] private BoxCollider _deliveryRoadCollider;
        [SerializeField] private Camera _deliveryCam;
        
        private GameObject _playerRoot;
        private Camera _cam;

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
                Vector3 newPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                _playerPositionController.SetNewPos(newPos);
                _playerPositionController.SetIsRiding(false);
                _pizzaThrowing.SetOnDestination(true);

                FindNumberOfClients();
                TurnOnDeliveryCamera();
            }
        }
        
        public void TurnOnMainCamera()
        {
            TurnOnRoadCollider();
            _cam.enabled = true;
            _deliveryCam.enabled = false;
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
            _pizzaThrowing.SetNumberOfClients(_listOfClients.transform.childCount);
            _pizzaThrowing.SetNumberOfThrowingChance(_pizzaThrowing.NumberOfClients * 2);   //как починить логику? В скрипте OnDestination может находится свойство с значением numberOfClients?
        }
    }
}
