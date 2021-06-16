using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DebrisPool : MonoBehaviour
{
    [SerializeField] private DebrisManager.DebrisType _type;
    [SerializeField] private Debris Prefab;
    
    public DebrisManager.DebrisType Type => _type;
    public bool IsReady => PhotonNetwork.IsConnectedAndReady;

    private readonly List<Debris> _available = new List<Debris>();
    private readonly List<Debris> _inUse = new List<Debris>();

    protected bool _isInitialised = false;

    private void Awake()
    {
        Debris[] children = transform.GetComponentsInChildren<Debris>(true);
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

    public Debris GetObject(Vector3 position, Quaternion rotation)
    {
        if (_available.Count > 0)
        {
            Debris obj = _available[0];
            _inUse.Add(obj);
            _available.RemoveAt(0);
            obj.Get();
            return obj;
        }
        else
        {
            GameObject obj = PhotonNetwork.Instantiate(Prefab.name, position, rotation);
            if (obj != null)
            {
                Debris debris = obj.GetComponent<Debris>();
                if (debris != null) _inUse.Add(debris);
                return debris;
            }

            return null;
        }
    }

    public void ReleaseObject(Debris obj)
    {
        obj.Release();
        _available.Add(obj);
        _inUse.Remove(obj);
    }
}
