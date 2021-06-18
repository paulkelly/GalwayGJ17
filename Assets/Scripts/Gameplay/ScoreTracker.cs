using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ScoreTracker : MonoBehaviourPun, IPunObservable
{
    public delegate void ScoreUpdated(int value);
    public static event ScoreUpdated OnScoreUpdated; 
    public static int Score { get; private set; }

    private void OnEnable()
    {
        if (photonView.IsMine)
        {
            Score = 0;
            Debris.OnDebrisDestroyed += UpdateScore;
        }
    }

    private void OnDisable()
    {
        if (photonView.IsMine)
        {
            Debris.OnDebrisDestroyed -= UpdateScore;
        }
    }

    private void UpdateScore(int value)
    {
        Score += value;
        OnScoreUpdated?.Invoke(Score);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Score);
        }
        else
        {
            Score = (int) stream.ReceiveNext();
        }
    }
}
