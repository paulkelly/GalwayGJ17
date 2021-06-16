using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPooledObject
{
    public void Get();
    public void Release();
}
