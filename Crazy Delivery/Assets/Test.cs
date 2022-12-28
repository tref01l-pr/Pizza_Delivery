using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private GameObject _player;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _player.SetActive(false);
        }

        if (Input.GetMouseButtonDown(1))
        {
            _player.SetActive(true);
        }
    }
}
