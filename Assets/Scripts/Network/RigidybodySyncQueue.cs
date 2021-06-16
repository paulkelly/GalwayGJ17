using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidybodySyncQueue : MonoBehaviour
{
    private const int NumberToSyncPerUpdate = 2;

    private static RigidybodySyncQueue _instance;
    private static List<SyncedRigidbody> _syncQueue = new List<SyncedRigidbody>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnGameStart()
    {
        GameObject obj = new GameObject("Rigidbody Sync");
        obj.AddComponent<RigidybodySyncQueue>();
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Subscribe(SyncedRigidbody rigidbody)
    {
        _syncQueue.Add(rigidbody);
    }

    public static void UnSubscribe(SyncedRigidbody rigidbody)
    {
        _syncQueue.Remove(rigidbody);
    }

    private void Update()
    {
        int toProcess = Math.Max(NumberToSyncPerUpdate, _syncQueue.Count);
        for (int i = 0; i < toProcess; i++)
        {
            SyncedRigidbody toSync = _syncQueue[0];
            toSync.Sync();
            _syncQueue.RemoveAt(0);
            _syncQueue.Add(toSync);
        }
    }
}
