using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PooledObject : MonoBehaviour
{
    public void Get()
    {
        gameObject.SetActive(true);
    }

    public void Release()
    {
        gameObject.SetActive(false);
    }
}
