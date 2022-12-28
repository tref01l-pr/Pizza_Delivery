using UnityEngine;

public class RagDollController : MonoBehaviour
{
    [SerializeField] private Rigidbody[] allRigidbodys;
    
    [SerializeField] private Animator _animator;
    
    public void MakePhysical()
    {
        _animator.enabled = false;
        for (int i = 0; i < allRigidbodys.Length; i++)
        {
            allRigidbodys[i].isKinematic = false;
        }
    }
    
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
}
