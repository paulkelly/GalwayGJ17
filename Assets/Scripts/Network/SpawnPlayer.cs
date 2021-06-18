using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.SpawnPlayer();
    }
}
