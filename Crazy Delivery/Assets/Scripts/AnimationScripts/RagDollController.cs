using UnityEngine;

namespace AnimationScripts
{
    public class RagDollController : MonoBehaviour
    {
        [SerializeField] private Rigidbody[] allRigidbodys;
    
        [SerializeField] private Animator animator;
    
        public void MakePhysical()
        {
            animator.enabled = false;
            foreach (var rigidobody in allRigidbodys)
            {
                rigidobody.isKinematic = false;
            }
        }
    
        private void Awake()
        {
            animator.Play("mixamo_com");
        }
    
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Pizza"))
            {
                MakePhysical();
            }
        }
    }
}
