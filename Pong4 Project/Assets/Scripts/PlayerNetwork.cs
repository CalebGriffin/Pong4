using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNetwork : NetworkBehaviour
{
    float lowerBound;
    float upperBound;

    float moveSpeed = 4f;

    bool gameRunning = false;

    private GameObject gameCanvas;
    private Button playButton;
    private TextMeshProUGUI playersText;

    private GameObject waitingCanvas;

    [SerializeField] private GameObject[] wallArr = new GameObject[4];

    [SerializeField] private Color[] colorArr = new Color[4];

    void Awake()
    {
        lowerBound = -5f + (transform.localScale.y / 2);
        upperBound = 5f - (transform.localScale.y / 2);
    }

    public override void OnNetworkSpawn()
    {
        int playerIndex = NetworkManager.Singleton.ConnectedClientsIds.Count - 1;
        transform.parent.localEulerAngles = new Vector3(0f, 0f, (playerIndex % 4) * 90f);
        gameObject.GetComponent<SpriteRenderer>().color = colorArr[playerIndex % 4];

        if (IsHost || IsServer) OnHostSpawn();
        else OnClientSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (IsHost || IsServer) OnHostDespawn();
    }

    void OnHostSpawn()
    {
        // This is run when the first player joins
        gameCanvas = GRefs.Singleton.gameCanvas;
        gameCanvas.SetActive(true);
        playButton = GRefs.Singleton.playGameButton;
        playButton.onClick.AddListener(PlayGame);
        playersText = GRefs.Singleton.playersText;
    }

    void OnClientSpawn()
    {
        waitingCanvas = GRefs.Singleton.waitingCanvas;
        waitingCanvas.SetActive(true);
    }

    void OnHostDespawn()
    {
        // This is run when the last player leaves
    }

    void Update()
    {
        if (IsHost) HostUpdate();
        if (!IsOwner) return;
        HandleMovement();
    }

    void HostUpdate()
    {
        if (!gameRunning) playersText.text = $"Players: {NetworkManager.Singleton.ConnectedClientsIds.Count}";
    }

    [ServerRpc(RequireOwnership = false)]
    void DespawnWallServerRpc(int wallIndex, ServerRpcParams serverRpcParams = default)
    {
        wallArr[wallIndex].GetComponent<NetworkObject>().Despawn(false);
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnWallServerRpc(int wallIndex, ServerRpcParams serverRpcParams = default)
    {
        GameObject wallObj = Instantiate(wallArr[wallIndex], wallArr[wallIndex].transform.position, Quaternion.identity);
        wallObj.GetComponent<NetworkObject>().Spawn();
    }

    void HandleMovement()
    {
        if (Input.GetKey(KeyCode.LeftArrow)) transform.position = !Mathf.Approximately(transform.parent.localEulerAngles.z, 90f) ? MovePaddle(-transform.up * moveSpeed * Time.deltaTime) : MovePaddle(transform.up * moveSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.RightArrow)) transform.position = !Mathf.Approximately(transform.parent.localEulerAngles.z, 90f) ? MovePaddle(transform.up * moveSpeed * Time.deltaTime) : MovePaddle(-transform.up * moveSpeed * Time.deltaTime);
    }

    Vector3 MovePaddle(Vector3 moveAmount)
    {
        float x = 0f;
        float y = 0f;

        if (Mathf.Approximately(transform.parent.localEulerAngles.z, 0f)) x = 4.9f;
        else if (Mathf.Approximately(transform.parent.localEulerAngles.z, 90f)) y = 4.9f;
        else if (Mathf.Approximately(transform.parent.localEulerAngles.z, 180f)) x = -4.9f;
        else if (Mathf.Approximately(transform.parent.localEulerAngles.z, 270f)) y = -4.9f;

        if (x == 0f) x = Mathf.Clamp(transform.position.x + moveAmount.x, lowerBound, upperBound);
        if (y == 0f) y = Mathf.Clamp(transform.position.y + moveAmount.y, lowerBound, upperBound);

        return new Vector3(x, y, transform.position.z);
    }

    void PlayGame()
    {
        gameRunning = true;
        for (int i = wallArr.Length - 1; i >= NetworkManager.Singleton.ConnectedClientsIds.Count; i--)
        {
            SpawnWallServerRpc(i);
        }
    }
}