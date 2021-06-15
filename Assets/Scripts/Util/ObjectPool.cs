using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> : MonoBehaviour where T : PooledObject
{
    public PooledObject Type;

    private readonly List<T> _available = new List<T>();
    private readonly List<T> _inUse = new List<T>();

    protected bool _isInitialised = false;

    private void Awake()
    {
        T[] children = transform.GetComponentsInChildren<T>(true);
        for (int i = 0; i < children.Length; i++)
        {
            _available.Add(children[i]);
            children[i].Release();
        }
        OnAwake();

        _isInitialised = true;
    }

    public virtual void OnAwake()
    {
        
    }

    public T GetObject()
    {
        if (_available.Count > 0)
        {
            T obj = _available[0];
            _inUse.Add(obj);
            _available.RemoveAt(0);
            obj.Get();
            return obj;
        }
        else
        {
            T obj = (T) Instantiate(Type, transform);
            _inUse.Add(obj);
            return obj;
        }
    }

    public void ReleaseObject(T obj)
    {
        obj.Release();
        _available.Add(obj);
        _inUse.Remove(obj);
    }
}
