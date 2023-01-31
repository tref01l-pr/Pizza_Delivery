using UnityEngine;
using UnityEngine.UI;

namespace OnDeliveryDestinationScripts
{
    public class JoystickOnDestination : MonoBehaviour
    {
        [SerializeField] private PlayerPositionController _playerPositionController;
        [SerializeField] private Image _joystickImage;

        private void Update()
        {
            if (_playerPositionController.IsRiding)
            {
                _joystickImage.enabled = true;
            }
            else
            {
                _joystickImage.enabled = false;
            }
        }
    }
}
