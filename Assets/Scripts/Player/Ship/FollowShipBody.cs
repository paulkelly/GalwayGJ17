using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowShipBody : MonoBehaviour
{
    [SerializeField] Transform _shipBody;

    private void Update()
    {
        transform.position = _shipBody.position;
    }
}
