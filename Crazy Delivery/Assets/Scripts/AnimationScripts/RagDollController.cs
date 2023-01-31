using UnityEngine;

namespace AnimationScripts
{
    public class RagDollController : MonoBehaviour
    {
        [SerializeField] private Rigidbody[] _allRigidbodys;
        [SerializeField] private Animator _animator;
        
        private void Awake()
        {
            _animator.Play("mixamo_com");
        }
    
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Pizza"))
            {
                MakePhysical();
            }
        }
        
        public void MakePhysical()
        {
            _animator.enabled = false;
            foreach (var rigidobody in _allRigidbodys)
            {
                rigidobody.isKinematic = false;
            }
        }
    }
}
