using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button serverButton;
    [SerializeField] private Button clientButton;

    private bool isHost = false;
    private bool isServer = false;
    private bool isClient = false;

    void Awake()
    {
        hostButton.onClick.AddListener(Host);
        serverButton.onClick.AddListener(Server);
        clientButton.onClick.AddListener(Client);
    }

    private void Host()
    {
        if (isServer || isClient)
        {
            return;
        }

        if (isHost)
        {
            NetworkManager.Singleton.Shutdown();
            hostButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start Host";
            isHost = false;
        }
        else
        {
            NetworkManager.Singleton.StartHost();
            hostButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stop Host";
            isHost = true;
        }
    }

    private void Server()
    {
        if (isHost || isClient)
        {
            return;
        }

        if (isServer)
        {
            NetworkManager.Singleton.Shutdown();
            serverButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start Server";
            isServer = false;
        }
        else
        {
            NetworkManager.Singleton.StartServer();
            serverButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stop Server";
            isServer = true;
        }
    }

    private void Client()
    {
        if (isHost || isServer)
        {
            return;
        }


        if (isClient)
        {
            NetworkManager.Singleton.Shutdown();
            clientButton.GetComponentInChildren<TextMeshProUGUI>().text = "Start Client";
            isClient = false;
        }
        else
        {
            NetworkManager.Singleton.StartClient();
            clientButton.GetComponentInChildren<TextMeshProUGUI>().text = "Stop Client";
            isClient = true;
        }
    }
}
