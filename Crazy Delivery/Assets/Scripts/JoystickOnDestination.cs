using UnityEngine;
using UnityEngine.UI;


public class JoystickOnDestination : MonoBehaviour
{
    [SerializeField] private PlayerPositionController _playerPositionController;
    
    [SerializeField] private Image joystickImage;

    private void Start()
    {
        joystickImage = GetComponent<Image>();
    }

    private void FixedUpdate()
    {
        if (_playerPositionController.isRiding)
        {
            joystickImage.enabled = true;
        }
        else
        {
            joystickImage.enabled = false;
        }
    }
}
