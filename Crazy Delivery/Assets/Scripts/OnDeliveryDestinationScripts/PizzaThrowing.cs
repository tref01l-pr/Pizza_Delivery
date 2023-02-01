using UnityEngine;

namespace OnDeliveryDestinationScripts
{
    public class PizzaThrowing : MonoBehaviour
    {
        [SerializeField] private GameObject _pizzaPrefab;
        [SerializeField] private GameObject _pizzaSight; 
        [SerializeField] private MeshRenderer _pizzaSightMeshRenderer;
        [SerializeField] private OnDeliveryDestination _onDeliveryDestination;
        [SerializeField] private ScoreManager _scoreManager;
    
        private float _speedRotationSight = 15f;
        private float _speedErroreEclusion;
        private int _numberOfThrowingChance;
        private bool _canSpawnPizza = true;
        private bool _canSpawnPizzaSight = true;
        private bool _onDestination;
        private GameObject _pizzaSpawn;
        private GameObject _player;
        private Vector3 _spawnPosition;
        private PlayerPositionController _playerPositionController;

        public int NumberOfClients { get; private set; }

        private void Start()
        {
            _scoreManager = GameObject.Find("Score").GetComponent<ScoreManager>();
            _speedErroreEclusion = _speedRotationSight * 2;
            _player = GameObject.Find("PushBikeWRagdoll");
            _playerPositionController = _player.GetComponent<PlayerPositionController>();
            _playerPositionController.SetIsRiding(true);
            _spawnPosition = transform.position;
            _spawnPosition.y -= 0.5f;
            _pizzaSightMeshRenderer.enabled = false;
        }

        private void Update()
        {
            if (_onDestination)
            {
                RotateSight();
            
                if (_canSpawnPizza)
                {
                    GetInput();
                }
       
                if (NumberOfClients == 0 || _numberOfThrowingChance < 0)
                {
                    _playerPositionController.SetIsRiding(true);
                    EndPizzaThrowing();
                }
            }
        }

        
        
        public void SetNumberOfThrowingChance(int numberOfThrowingChance)
        {
            _numberOfThrowingChance = numberOfThrowingChance;
        }

        public void SetNumberOfClients(int numbersOfClients)
        {
            NumberOfClients = numbersOfClients;
        }

        public void SetCanSpawnPizza(bool canSpawnPizza)
        {
            _canSpawnPizza = canSpawnPizza;
        }

        /*public void SetPizzaSightSpawn(bool pizzaSightSpawn)
        {
            _canSpawnPizzaSight = pizzaSightSpawn;
        }*/
        

        public void SetOnDestination(bool onDestination)
        {
            _onDestination = onDestination;
        }
        
        public void EnableGravity()
        {
            _pizzaSpawn.GetComponent<Rigidbody>().useGravity = true;
        }
        
        private void GetInput()
        {
            if (Input.GetMouseButtonDown(0))
            { 
                _numberOfThrowingChance--;
                _canSpawnPizza = false;
                if (NumberOfClients > 0 && _numberOfThrowingChance >= 0)
                {
                    SpawnPizza();
                }
            }
        }

        private void SpawnPizza()
        {
            _pizzaSpawn = Instantiate(_pizzaPrefab, _spawnPosition, 
                Quaternion.Euler(0, _pizzaSight.transform.localEulerAngles.y, 0), transform.root);
            
            _pizzaSpawn.GetComponent<CheckCollision>().Init(this);
            _pizzaSpawn.GetComponent<Collider>().enabled = true;
        }
    
        
        
    
        private void RotateSight()
        {
            SpawnPizzaSight();

            AvoidInaccuracy();

            _pizzaSight.transform.Rotate(Vector3.up * _speedRotationSight * Time.deltaTime);
        }

        private void SpawnPizzaSight()
        {
            if (_canSpawnPizzaSight)
            {
                _pizzaSight.transform.localEulerAngles =
                    new Vector3(_pizzaSight.transform.localEulerAngles.x, 90, _pizzaSight.transform.localEulerAngles.z);
                
                _pizzaSightMeshRenderer.enabled = true;
                _canSpawnPizzaSight = false;
            }
        }

        private void AvoidInaccuracy()
        {
            if (_pizzaSight.transform.localEulerAngles.y < 75f || _pizzaSight.transform.localEulerAngles.y > 105f)
            {
                _speedRotationSight = -_speedRotationSight;
                _speedErroreEclusion = -_speedErroreEclusion;
                _pizzaSight.transform.Rotate(Vector3.up * _speedErroreEclusion * Time.deltaTime);
            }
        }

        private void EndPizzaThrowing()
        {
            while (_numberOfThrowingChance > 0)
            {
                _scoreManager.AddPoint();
                _numberOfThrowingChance--;
            }
            
            _onDestination = false;
            _pizzaSightMeshRenderer.enabled = false;
            _onDeliveryDestination.TurnOnMainCamera();
        }
    }
}
