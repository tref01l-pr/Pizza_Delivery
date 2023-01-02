using UnityEngine;

public class PlayerPositionController : MonoBehaviour
{
    [HideInInspector] public Vector3 newPos;
    public bool isRiding { get; set; } = true;
    public bool teleportPlaye { get; set; }= false;

    [SerializeField] private GameObject player;
    [SerializeField] private Rigidbody[] playerRigidbody;
    [SerializeField] private Joystick _joystick; 


    public void TeleportPlayerOnDestination()
    {
        player.transform.position = newPos;
    }
    
    public void DisActivateBicycle()
    {
        for (int i = 0; i < playerRigidbody.Length; i++) 
        { 
            playerRigidbody[i].velocity = Vector3.zero; 
            playerRigidbody[i].angularVelocity = Vector3.zero;
        }
        player.transform.eulerAngles = new Vector3(0f, 90f, 0f);

        TeleportPlayerOnDestination();
    }
    
    private void Update()
    {
        if (isRiding == false)
        {
            DisActivateBicycle();
        }
    }
}
