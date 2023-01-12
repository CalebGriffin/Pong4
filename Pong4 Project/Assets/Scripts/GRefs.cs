using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GRefs : MonoBehaviour
{
    public static GRefs Singleton;

    public GameObject gameCanvas;
    public GameObject waitingCanvas;
    public Button playGameButton;
    public TextMeshProUGUI playersText;

    void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
