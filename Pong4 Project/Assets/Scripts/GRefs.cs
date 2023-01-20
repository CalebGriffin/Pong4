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
    public GameObject scoreCanvas;
    public Button playGameButton;
    public TextMeshProUGUI playersText;
    public TextMeshProUGUI scoreText;

    public int score = 0;

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
