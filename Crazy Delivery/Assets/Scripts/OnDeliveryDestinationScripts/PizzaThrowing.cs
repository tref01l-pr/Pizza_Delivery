using UnityEngine;

namespace OnDeliveryDestinationScripts
{
    public class PizzaThrowing : MonoBehaviour
    {
        public bool OnDestination { get; set; }
        public bool IsPizza { get; set; }
        public bool PizzaSightSpawn { get; set; }
        public bool CanSpawnPizza { get; set; } = true;
        public int NumberOfClients { get; set; }
        public int NumberOfThrowingChance { get; set; }
        public GameObject PizzaSpawn { get; private set; }
        
        [SerializeField] private GameObject _pizza;
        [SerializeField] private GameObject _pizzaSight;
        [SerializeField] private GameObject _playerRoot;
        [SerializeField] private MeshRenderer _pizzaSightMeshRenderer;
        
        private Vector3 _spawnPos;
    
        [SerializeField] private OnDeliveryDestination _onDeliveryDestination;
        [SerializeField] private PlayerPositionController _playerPositionController;
        [SerializeField] private ScoreManager _scoreManager;
    
        private float _speedRotationSight = 15;
        private float _speedOfFlying = 30;
        private float _speedErroreEclusion;

        public void EnableGravity()
        {
            PizzaSpawn.GetComponent<Rigidbody>().useGravity = true;
        }

        private void Start()
        {
            _scoreManager = GameObject.Find("Score").GetComponent<ScoreManager>();
            _speedErroreEclusion = _speedRotationSight * 2;
            _playerRoot = GameObject.Find("PushBikeWRagdoll");
            _playerPositionController = _playerRoot.GetComponent<PlayerPositionController>();
            _playerPositionController.IsRiding = true;
            _spawnPos = transform.position;
            _spawnPos.y -= 0.5f;
            _pizzaSightMeshRenderer.enabled = false;
        }

        private void Update()
        {
            if (OnDestination)
            {
                SightRotation();
            
                if (CanSpawnPizza)
                {
                    GetInput();
                }
       
                if (NumberOfClients == 0 || NumberOfThrowingChance < 0)
                {
                    _playerPositionController.IsRiding = true;
                    EndPizzaThrowing();
                }
            }
        }

        private void FixedUpdate()
        {
            if (OnDestination)
            {
                if (IsPizza)
                {
                    PizzaFly();
                }
            }
        }
    

        private void GetInput()
        {
            if (Input.GetMouseButtonDown(0))
            { 
                NumberOfThrowingChance--;
                CanSpawnPizza = false;
                if (NumberOfClients > 0 && NumberOfThrowingChance >= 0)
                {
                    PizzaSpawn = Instantiate(_pizza, _spawnPos, 
                        Quaternion.Euler(0, _pizzaSight.transform.localEulerAngles.y, 0), transform.root);
                    PizzaSpawn.GetComponent<CheckCollision>().Init(this);
                    IsPizza = true;
                    PizzaSpawn.GetComponent<Collider>().enabled = true;
                }
            }
        }
    
        private void PizzaFly()
        {
            if (transform.position.z < 0)
            {
                PizzaSpawn.transform.localPosition += PizzaSpawn.transform.right * _speedOfFlying * Time.deltaTime;
            }
            else
            {
                PizzaSpawn.transform.localPosition -= PizzaSpawn.transform.right * _speedOfFlying * Time.deltaTime;
            }
        }
    
        private void SightRotation()
        {
            if (PizzaSightSpawn)
            {
                _pizzaSight.transform.localEulerAngles =
                    new Vector3(_pizzaSight.transform.localEulerAngles.x, 90, _pizzaSight.transform.localEulerAngles.z);
                _pizzaSightMeshRenderer.enabled = true;
                PizzaSightSpawn = false;
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
            while (NumberOfThrowingChance > 0)
            {
                _scoreManager.AddPoint();
                NumberOfThrowingChance--;
            }
            OnDestination = false;
            _pizzaSightMeshRenderer.enabled = false;
            _onDeliveryDestination.TurnOnMainCamera();
        }
    }
}
