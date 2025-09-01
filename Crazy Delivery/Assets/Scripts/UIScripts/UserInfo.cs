using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserInfo : MonoBehaviour
{
    [SerializeField]
    private TMP_Text userName;
    public void OnEnable()
    {
        userName.text = PlayerPrefs.GetString(PlayerManager.PREF_USERNAME);
        Debug.Log("User Name: " + userName);
    }
}
