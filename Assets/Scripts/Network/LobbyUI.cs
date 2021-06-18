using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private List<Button> _allButtons;
    [SerializeField] private List<Button> _buttonsToDisableWhenOffline;

    [SerializeField] private GameObject _connectingMessage;
    [SerializeField] private GameObject _offlineMessage;

    private void Update()
    {
        if (NetworkManager.IsBusy || NetworkManager.IsConnecting)
        {
            foreach (var button in _allButtons)
            {
                button.interactable = false;
            }
        }
        else
        {
            bool offline = NetworkManager.IsOffline;
            foreach (var button in _allButtons)
            {
                button.interactable = !offline || !_buttonsToDisableWhenOffline.Contains(button);
            }
        }
        
        _connectingMessage.SetActive(NetworkManager.IsConnecting);
        _offlineMessage.SetActive(!NetworkManager.IsConnecting && NetworkManager.IsOffline);
    }
}
