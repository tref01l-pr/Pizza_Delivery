using UnityEngine;

namespace OnDeliveryDestinationScripts
{
    public class PizzaThrowing : MonoBehaviour
    {
        
        [SerializeField] private GameObject _pizza;
        [SerializeField] private GameObject _pizzaSight;
        [SerializeField] private GameObject _playerRoot;
        [SerializeField] private MeshRenderer _pizzaSightMeshRenderer;
        [SerializeField] private OnDeliveryDestination _onDeliveryDestination;
        [SerializeField] private PlayerPositionController _playerPositionController;
        [SerializeField] private ScoreManager _scoreManager;
    
        private float _speedRotationSight = 15;
        private float _speedOfFlying = 30;
        private float _speedErroreEclusion;
        private int _numberOfThrowingChance;
        private bool _canSpawnPizza = true;
        private bool _pizzaSightSpawn;
        private bool _isPizza;
        private bool _onDestination;
        private GameObject _pizzaSpawn;
        private Vector3 _spawnPos;

        public int NumberOfClients { get; private set; }

        private void Start()
        {
            _scoreManager = GameObject.Find("Score").GetComponent<ScoreManager>();
            _speedErroreEclusion = _speedRotationSight * 2;
            _playerRoot = GameObject.Find("PushBikeWRagdoll");
            _playerPositionController = _playerRoot.GetComponent<PlayerPositionController>();
            _playerPositionController.SetIsRiding(true);
            _spawnPos = transform.position;
            _spawnPos.y -= 0.5f;
            _pizzaSightMeshRenderer.enabled = false;
        }

        private void Update()
        {
            if (_onDestination)
            {
                SightRotation();
            
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

        private void FixedUpdate()
        {
            if (_onDestination)
            {
                if (_isPizza)
                {
                    PizzaFly();
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

        public void SetPizzaSightSpawn(bool pizzaSightSpawn)
        {
            _pizzaSightSpawn = pizzaSightSpawn;
        }

        public void SetIsPizza(bool isPizza)
        {
            _isPizza = isPizza;
        }

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
                    _pizzaSpawn = Instantiate(_pizza, _spawnPos, 
                        Quaternion.Euler(0, _pizzaSight.transform.localEulerAngles.y, 0), transform.root);
                    _pizzaSpawn.GetComponent<CheckCollision>().Init(this);
                    _isPizza = true;
                    _pizzaSpawn.GetComponent<Collider>().enabled = true;
                }
            }
        }
    
        private void PizzaFly()
        {
            if (transform.position.z < 0)
            {
                _pizzaSpawn.transform.localPosition += _pizzaSpawn.transform.right * _speedOfFlying * Time.deltaTime;
            }
            else
            {
                _pizzaSpawn.transform.localPosition -= _pizzaSpawn.transform.right * _speedOfFlying * Time.deltaTime;
            }
        }
    
        private void SightRotation()
        {
            if (_pizzaSightSpawn)
            {
                _pizzaSight.transform.localEulerAngles =
                    new Vector3(_pizzaSight.transform.localEulerAngles.x, 90, _pizzaSight.transform.localEulerAngles.z);
                _pizzaSightMeshRenderer.enabled = true;
                _pizzaSightSpawn = false;
            } 
        
            if (_pizzaSight.transform.localEulerAngles.y < 75f || _pizzaSight.transform.localEulerAngles.y > 105f)
            {
                _speedRotationSight = -_speedRotationSight;
                _speedErroreEclusion = -_speedErroreEclusion;
                _pizzaSight.transform.Rotate(Vector3.up * _speedErroreEclusion * Time.deltaTime);
            }
        
        
            _pizzaSight.transform.Rotate(Vector3.up * _speedRotationSight * Time.deltaTime);
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
