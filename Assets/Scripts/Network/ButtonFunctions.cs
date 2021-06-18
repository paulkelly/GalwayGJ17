using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonFunctions : MonoBehaviour
{
    public void Quit()
    {
        Application.Quit();
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
