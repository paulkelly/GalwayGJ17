using System.Collections;
using System.Collections.Generic;
using Billygoat;
using UnityEngine;

public class ButtonFunctions : MonoBehaviour
{
    public void Quit()
    {
        BGSceneLoader.Quit();
    }
    
    public void ReturnToLobby()
    {
        NetworkManager.ReturnToLobby();
    }

    public void HostPublicGame()
    {
        NetworkManager.HostGame();
    }
    
    public void HostSoloGame()
    {
        NetworkManager.StartSoloGame();
    }

    public void JoinAnyGame()
    {
        NetworkManager.JoinAnyGame();
    }
}
