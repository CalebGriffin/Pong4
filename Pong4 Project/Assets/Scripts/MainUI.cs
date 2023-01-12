using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainUI : MonoBehaviour
{
    [SerializeField] private Button joinGameButton;

    void Awake()
    {
        joinGameButton.onClick.AddListener(JoinGame);
    }

    void JoinGame()
    {
        #if UNITY_EDITOR
        NetworkManager.Singleton.StartHost();
        #else
        NetworkManager.Singleton.StartClient();
        #endif

        gameObject.SetActive(false);
    }
}
