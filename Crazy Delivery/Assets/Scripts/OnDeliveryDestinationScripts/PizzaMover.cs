using UnityEngine;

namespace OnDeliveryDestinationScripts
{
    public class PizzaMover : MonoBehaviour
    {
        private float _speedOfFlying = 30f;
        private void FixedUpdate()
        { 
            Move();
        }

        private void Move()
        {
            if (transform.position.z < 0)
            {
                transform.localPosition += transform.right * (_speedOfFlying * Time.fixedDeltaTime);
            }
            else
            {
                transform.localPosition -= transform.right * (_speedOfFlying * Time.fixedDeltaTime);
            }
        }
    }
}
