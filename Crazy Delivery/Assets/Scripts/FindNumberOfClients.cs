using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindNumberOfClients : MonoBehaviour
{
    
    private void FindNumberOfClientsFunction()
    {
        string nameOfParentObject = name;
        string nameOfChildObject = "XBot";

        string childLocation = "/" + nameOfParentObject + "/" + nameOfChildObject;
        GameObject childObject = GameObject.Find (childLocation);
    }
}
